using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Hunting;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Ritmo do Tambor of the Tambor Rubro (11, <see cref="DrumRhythmRules"/>), on the same driver as the sandstorm: every 10 min (UTC) the
    /// drums play for 60 s; in the city the hero gets a short StaminaRegen effect refreshed while they play (it ends by itself if the driver
    /// stops), and a Tarefa de caça completed in the Ermos do Tambor Rubro while they play gives +1 Ponto de Caça
    /// (<see cref="HuntingProgress.AddBonusHuntPoints"/>). A cousin or Ashmunikal says a line once per beat in the city.
    /// </summary>
    public sealed partial class AreiasMechanicsDriver
    {
        private const string DrumStaminaSource = "tambor_rubro_drum";

        private StatModifierDefinition _drumStamina;
        private bool _drumSubscribed;
        private bool _inBadlands;
        private bool _drumOn;
        private long _drumSaid = long.MinValue;
        private long _drumWarned = long.MinValue;
        private long _drumLine = long.MinValue;
        private float _nextDrumRefresh;

        /// <summary>The drums and the oasis listen to the EventBus (task completions, the hero's hits) while the driver is on.</summary>
        private void OnEnable()
        {
            SubscribeDrum();
            _oasisCombat.Attach();
        }

        private void SubscribeDrum()
        {
            if (!_drumSubscribed)
            {
                _drumSubscribed = true;
                EventBus.Subscribe<HuntingTaskCompletedEvent>(HandleDrumTask);
            }
        }

        /// <summary>Stops the drums' effect and the listening of the drums and the oasis (OnDisable).</summary>
        private void StopDrum()
        {
            if (_drumSubscribed)
            {
                _drumSubscribed = false;
                EventBus.Unsubscribe<HuntingTaskCompletedEvent>(HandleDrumTask);
            }

            _oasisCombat.Detach();
            LeaveDrum();
            LeaveOasis();
        }

        private void TickDrum(Vector2 local)
        {
            _inBadlands = OverworldLayouts.TamborRubroBadlandsBounds.Contains(local);
            if (!_inBadlands)
            {
                LeaveDrum();
                return;
            }

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            bool playing = DrumRhythmRules.Playing(now);
            long beat = DrumRhythmRules.BeatIndex(now);
            bool inCity = DrumRhythmRules.CityRect(OverworldLayouts.TamborRubroOffset).Contains(local);

            if (DrumRhythmRules.Warning(now) && inCity && _drumWarned != beat)
            {
                _drumWarned = beat;
                SayDrum(DrumRhythmTexts.Warning);
            }

            if (playing && _drumSaid != beat)
            {
                _drumSaid = beat;
                SayDrum(inCity ? DrumRhythmTexts.Playing : DrumRhythmTexts.PlayingFar);
            }

            if (playing && inCity && _drumLine != beat && DrumRhythmRules.CycleTime(now) >= 4d)
            {
                _drumLine = beat;
                SayDrum(DrumRhythmTexts.LineFor(MapRequirementHeroState.Capture(null).Hero, beat));
            }

            if (!playing && _drumOn)
            {
                SayDrum(DrumRhythmTexts.Over);
            }

            _drumOn = playing;
            float factor = DrumRhythmRules.HeroStaminaFactor(playing, inCity, _stats.IsAlive);
            SetDrumStamina(factor > 1f);
        }

        private void LeaveDrum()
        {
            _inBadlands = false;
            _drumOn = false;
            SetDrumStamina(false);
        }

        private void SetDrumStamina(bool on)
        {
            if (_heroBuffs == null && _player != null)
            {
                _heroBuffs = _player.GetComponent<BuffController>();
            }

            if (_heroBuffs == null)
            {
                return;
            }

            if (!on)
            {
                if (_nextDrumRefresh > 0f)
                {
                    _heroBuffs.RemoveEffectsBySource(DrumStaminaSource);
                    _nextDrumRefresh = 0f;
                }

                return;
            }

            if (Time.time < _nextDrumRefresh)
            {
                return;
            }

            _drumStamina ??= new StatModifierDefinition
            {
                displayName = "Ritmo do Tambor",
                affectedStat = StatType.StaminaRegen,
                application = ModifierApplication.PercentAdditive,
                value = DrumRhythmRules.StaminaFactor - 1f,
                durationSeconds = SouthEffectSeconds,
                isDebuff = false,
            };
            _nextDrumRefresh = Time.time + SouthEffectRefresh;
            _heroBuffs.ApplyEffect(_drumStamina, DrumStaminaSource);
        }

        private void HandleDrumTask(HuntingTaskCompletedEvent evt)
        {
            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            int bonus = DrumRhythmRules.TaskBonusPoints(DrumRhythmRules.Playing(now), _inBadlands);
            HuntingProgress hunting = HuntingProgress.ResolveShared();
            if (bonus <= 0 || hunting == null)
            {
                return;
            }

            hunting.AddBonusHuntPoints(bonus);
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayDrumText(string.Format(DrumRhythmTexts.Get(localization, DrumRhythmTexts.TaskBonus), bonus));
        }

        private void SayDrum(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayDrumText(DrumRhythmTexts.Get(localization, key));
        }

        private void SayDrumText(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
