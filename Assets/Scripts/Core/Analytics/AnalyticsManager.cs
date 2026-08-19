using System;
using System.Collections.Generic;
using System.Text;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;

namespace Rapadura.Core.Analytics
{
    /// <summary>
    /// One recorded gameplay analytics event: a name, a timestamp, and an arbitrary bag of
    /// properties (kept as plain objects so callers don't need a schema up front).
    /// </summary>
    public readonly struct AnalyticsEvent
    {
        public readonly string Name;
        public readonly DateTime TimestampUtc;
        public readonly IReadOnlyDictionary<string, object> Properties;

        public AnalyticsEvent(string name, DateTime timestampUtc, IReadOnlyDictionary<string, object> properties)
        {
            Name = name;
            TimestampUtc = timestampUtc;
            Properties = properties;
        }
    }

    /// <summary>
    /// Fase 10 ("Qualidade") gameplay analytics collector. Listens to gameplay events already
    /// raised on the <see cref="EventBus"/> (combat deaths, XP/level changes, recipes crafted)
    /// and also exposes a manual <see cref="TrackEvent"/> API for ad-hoc instrumentation, buffering
    /// everything in memory with a timestamp.
    ///
    /// PRODUCT DECISION PENDING: there is no real analytics backend wired up yet (no
    /// Unity Analytics / Firebase / custom telemetry service). <see cref="FlushToLog"/> only
    /// writes the buffered events to <see cref="GameLogger"/> for now — swapping that method's
    /// body for an HTTP call to a real service later is a drop-in change that does not require
    /// touching any of the TrackEvent call sites. Whether/which third-party analytics service to
    /// use, and what user consent/opt-out flow it needs (see TODO.md Fase 10 → Legal/Compliance,
    /// "Termos de uso e política de privacidade"), is a product decision out of scope here.
    ///
    /// Like <see cref="Rapadura.Core.Audio.AudioManager"/>, this is a plain C# class (not a
    /// MonoBehaviour) implementing <see cref="IManager"/>; it is meant to be constructed and
    /// registered by <see cref="Rapadura.Core.Managers.GameManager"/> — see the note at the
    /// bottom of this file for the wiring that still needs to happen there (out of scope for
    /// this change: GameManager.cs was intentionally not edited).
    /// </summary>
    public class AnalyticsManager : IManager
    {
        private const string LogCategory = "Analytics";

        private readonly List<AnalyticsEvent> _buffer = new List<AnalyticsEvent>();
        private readonly Func<DateTime> _utcNowProvider;

        public IReadOnlyList<AnalyticsEvent> BufferedEvents => _buffer;

        public AnalyticsManager(Func<DateTime> utcNowProvider = null)
        {
            _utcNowProvider = utcNowProvider ?? (() => DateTime.UtcNow);
        }

        public void Initialize()
        {
            EventBus.EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.EventBus.Subscribe<PlayerExperienceChangedEvent>(OnPlayerExperienceChanged);
            EventBus.EventBus.Subscribe<RecipeCraftedEvent>(OnRecipeCrafted);
        }

        public void Shutdown()
        {
            EventBus.EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnPlayerExperienceChanged);
            EventBus.EventBus.Unsubscribe<RecipeCraftedEvent>(OnRecipeCrafted);
        }

        /// <summary>Records a gameplay event into the in-memory buffer with the current timestamp.</summary>
        public void TrackEvent(string eventName, Dictionary<string, object> properties = null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            IReadOnlyDictionary<string, object> snapshot = properties != null
                ? new Dictionary<string, object>(properties)
                : EmptyProperties;

            _buffer.Add(new AnalyticsEvent(eventName, _utcNowProvider(), snapshot));
        }

        /// <summary>Writes every buffered event to <see cref="GameLogger"/> and clears the buffer.</summary>
        public void FlushToLog()
        {
            foreach (AnalyticsEvent evt in _buffer)
            {
                GameLogger.Info(LogCategory, FormatEvent(evt));
            }

            _buffer.Clear();
        }

        /// <summary>Discards all buffered events without logging them. Intended for tests/scene teardown.</summary>
        public void ClearBuffer()
        {
            _buffer.Clear();
        }

        private static readonly IReadOnlyDictionary<string, object> EmptyProperties = new Dictionary<string, object>();

        private static string FormatEvent(AnalyticsEvent evt)
        {
            var builder = new StringBuilder();
            builder.Append(evt.TimestampUtc.ToString("O"));
            builder.Append(" | ");
            builder.Append(evt.Name);

            if (evt.Properties.Count > 0)
            {
                builder.Append(" | ");
                bool first = true;

                foreach (KeyValuePair<string, object> kvp in evt.Properties)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(kvp.Key).Append('=').Append(kvp.Value);
                    first = false;
                }
            }

            return builder.ToString();
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            TrackEvent("combat_target_died", new Dictionary<string, object>
            {
                ["target"] = evt.Target != null ? evt.Target.name : "<null>",
                ["killer"] = evt.Killer != null ? evt.Killer.name : "<null>",
            });
        }

        private void OnPlayerExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            TrackEvent("player_experience_changed", new Dictionary<string, object>
            {
                ["currentExperience"] = evt.CurrentExperience,
                ["experienceToNextLevel"] = evt.ExperienceToNextLevel,
                ["level"] = evt.Level,
            });
        }

        private void OnRecipeCrafted(RecipeCraftedEvent evt)
        {
            TrackEvent("recipe_crafted", new Dictionary<string, object>
            {
                ["recipe"] = evt.Recipe != null ? evt.Recipe.name : "<null>",
            });
        }
    }
}

// ⚠️ Pendente (fora do escopo desta mudança — GameManager.cs não foi editado):
// registrar `AnalyticsManager` em `GameManager.BuildManagers()` (novo campo público +
// `new AnalyticsManager()` + `RegisterManager(...)`), no mesmo padrão de `AudioManager`.
// Também pendente: decisão de produto sobre qual serviço de analytics real usar e o fluxo de
// consentimento/opt-out do usuário (ver TODO.md Fase 10 → Legal/Compliance).
