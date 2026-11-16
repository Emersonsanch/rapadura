using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The season of the Tundra de Omöl (61, <see cref="TundraSeasonRules"/>), on the same driver as the canals: on the three areas of map 61
    /// (outside the Tenda de Yen's safe ground) a short-lived MoveSpeed effect follows the season — bog in summer, packed snow in winter —
    /// refreshed while it applies, so it ends by itself if the driver stops. The season is said on the way in and whenever it turns; nothing is
    /// saved (the in-game day is the calendar).
    /// </summary>
    public sealed partial class MarGeladoMechanicsDriver
    {
        private const string SeasonSource = "tundra_season_ground";
        private const float SeasonEffectSeconds = 1.5f;
        private const float SeasonEffectRefresh = 1f;

        private BuffController _seasonBuffs;
        private bool _inTundra;
        private int _seasonSaidDay = int.MinValue;
        private TundraSeason? _seasonSaid;
        private float _seasonApplied = 1f;
        private float _nextSeasonRefresh;

        private void TickSeason(Vector2 local, int day)
        {
            if (!OverworldLayouts.TundraSeasonArea.Contains(local) || _stats == null || !_stats.IsAlive)
            {
                LeaveSeason();
                return;
            }

            TundraSeason season = TundraSeasonRules.SeasonOf(day);
            bool safe = OverworldRules.SettlementAt(_layout, local + _origin) != null;
            // Said on the way in (once per in-game day, so walking the edge does not repeat it) and whenever the season turns.
            if ((!_inTundra && _seasonSaidDay != day) || (_inTundra && _seasonSaid.HasValue && _seasonSaid.Value != season))
            {
                SaySeason(season, day, _inTundra);
            }

            _inTundra = true;
            SetSeasonSpeed(TundraSeasonRules.SpeedFactor(season, safe));
        }

        private void SaySeason(TundraSeason season, int day, bool turned)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (turned)
            {
                SayText(TundraSeasonTexts.Get(localization, TundraSeasonTexts.Changed));
            }

            SayText(TundraSeasonTexts.Format(localization, TundraSeasonTexts.SeasonKey(season), TundraSeasonRules.DaysToNextSeason(day)));
            if (!turned && _seasonSaidDay != day)
            {
                string line = TundraSeasonTexts.LineFor(ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null);
                if (line != null)
                {
                    CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), TundraSeasonTexts.Get(localization, line));
                }
            }

            _seasonSaid = season;
            _seasonSaidDay = day;
        }

        private void SetSeasonSpeed(float factor)
        {
            if (_seasonBuffs == null && _player != null)
            {
                _seasonBuffs = _player.GetComponent<BuffController>();
            }

            if (_seasonBuffs == null)
            {
                return;
            }

            if (Mathf.Abs(factor - 1f) < 0.001f)
            {
                if (_seasonApplied != 1f)
                {
                    _seasonBuffs.RemoveEffectsBySource(SeasonSource);
                    _seasonApplied = 1f;
                }

                return;
            }

            if (Mathf.Abs(factor - _seasonApplied) < 0.001f && Time.time < _nextSeasonRefresh)
            {
                return;
            }

            if (_seasonApplied != 1f && (factor > 1f) != (_seasonApplied > 1f))
            {
                _seasonBuffs.RemoveEffectsBySource(SeasonSource);
            }

            _seasonApplied = factor;
            _nextSeasonRefresh = Time.time + SeasonEffectRefresh;
            _seasonBuffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = factor < 1f ? "Brejo" : "Neve batida",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = factor - 1f,
                durationSeconds = SeasonEffectSeconds,
                isDebuff = factor < 1f,
            }, SeasonSource);
        }

        /// <summary>Out of the tundra (or the driver stopping): the season's effect is taken off and the next entry says the season again.</summary>
        private void LeaveSeason()
        {
            _inTundra = false;
            if (_seasonApplied != 1f && _seasonBuffs != null)
            {
                _seasonBuffs.RemoveEffectsBySource(SeasonSource);
            }

            _seasonApplied = 1f;
        }
    }
}
