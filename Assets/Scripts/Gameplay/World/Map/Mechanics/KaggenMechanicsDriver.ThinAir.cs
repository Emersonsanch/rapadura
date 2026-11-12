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
    /// The thin air of the Cume dos Mazans (18, <see cref="MazanThinAirRules"/>), on the same driver as the Serra's winds: on the top plateau a
    /// short-lived StaminaRegen effect (−30 %) sits on the hero's BuffController, refreshed while it applies, so it ends by itself if the driver
    /// stops; Maithe never gets it. Deals no damage and never takes the controls.
    /// </summary>
    public sealed partial class KaggenMechanicsDriver
    {
        private const string ThinAirSource = "mazan_thin_air";
        private const float ThinAirEffectSeconds = 1.2f;
        private const float ThinAirRefresh = 0.5f;
        private const float ThinAirHintCooldown = 60f;

        private BuffController _heroBuffs;
        private StatModifierDefinition _thinAirEffect;
        private bool _thinAirOn;
        private bool _onPlateau;
        private bool _maitheHinted;
        private float _nextThinAirRefresh;
        private float _nextThinAirHint;

        private void TickThinAir(Vector2 local, CharacterId? hero)
        {
            bool onPlateau = OverworldLayouts.MazanThinAirBounds.Contains(local) && OverworldRules.SettlementAt(_layout, _origin + local) == null;
            if (!onPlateau)
            {
                LeaveThinAir();
                return;
            }

            bool entering = !_onPlateau;
            _onPlateau = true;
            bool thin = MazanThinAirRules.StaminaFactor(OverworldLayouts.MazanThinAirBounds, local, hero) < 1f;
            SetThinAir(thin);
            if (thin)
            {
                _maitheHinted = false;
                if (entering && Time.unscaledTime >= _nextThinAirHint)
                {
                    _nextThinAirHint = Time.unscaledTime + ThinAirHintCooldown;
                    SayThinAir(MazanThinAirTexts.ThinAir);
                }
            }
            else if (!_maitheHinted)
            {
                _maitheHinted = true;
                SayThinAir(MazanThinAirTexts.ThinAirMaithe);
            }
        }

        private void SetThinAir(bool on)
        {
            if (_heroBuffs == null)
            {
                _heroBuffs = _player != null ? _player.GetComponent<BuffController>() : null;
                if (_heroBuffs == null)
                {
                    return;
                }
            }

            if (!on)
            {
                if (_thinAirOn)
                {
                    _heroBuffs.RemoveEffectsBySource(ThinAirSource);
                    _thinAirOn = false;
                }

                return;
            }

            if (_thinAirOn && Time.time < _nextThinAirRefresh)
            {
                return;
            }

            _thinAirEffect ??= new StatModifierDefinition
            {
                displayName = "Ar fino",
                affectedStat = StatType.StaminaRegen,
                application = ModifierApplication.PercentAdditive,
                value = MazanThinAirRules.StaminaRegenBonus,
                durationSeconds = ThinAirEffectSeconds,
                isDebuff = true,
            };
            _thinAirOn = true;
            _nextThinAirRefresh = Time.time + ThinAirRefresh;
            _heroBuffs.ApplyEffect(_thinAirEffect, ThinAirSource);
        }

        private void LeaveThinAir()
        {
            if (!_onPlateau)
            {
                return;
            }

            bool wasThin = _thinAirOn;
            _onPlateau = false;
            _maitheHinted = false;
            SetThinAir(false);
            if (wasThin && _stats != null && _stats.IsAlive)
            {
                SayThinAir(MazanThinAirTexts.ThinAirEnd);
            }
        }

        private void SayThinAir(string key)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, MazanThinAirTexts.Get(localization, key));
        }
    }
}
