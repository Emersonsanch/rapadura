using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The still air of the Mercado dos Túneis (22, <see cref="TunnelStillAirRules"/>), on the same driver as the boat and the hall: far from the
    /// copper pipes a short-lived StaminaRegen effect (−100 %) sits on the hero's BuffController, refreshed while it applies (it ends by itself
    /// if the driver stops), and the stamina falls slowly through <c>PlayerStats.DrainStamina</c>, never under 30 %. An open Garrafa de Ar
    /// Sereno stops all of it. Short copper-green columns mark the pipes near the hero. Deals no damage and never takes the controls.
    /// </summary>
    public sealed partial class LimiarLakeMechanicsDriver
    {
        private const string StillAirSource = "tunnel_still_air";
        private const float StillAirRefresh = 0.5f;
        private const float StillAirHintCooldown = 45f;
        private const float PipeViewRange = 90f;
        private const float PipeHeight = 4f;

        private static readonly Color PipeColor = new Color(0.45f, 0.82f, 0.66f, 0.85f);

        private static readonly string BottleSource = InventoryManager.GetConsumableSourceKey(TunnelStillAirRules.BottleItemId);

        private BuffController _airBuffs;
        private StatModifierDefinition _stillAirEffect;
        private LineRenderer[] _pipeLines;
        private bool _inMarket;
        private bool _stillOn;
        private bool _bottleSaid;
        private float _nextStillRefresh;
        private float _nextStillHint;

        private void TickStillAir(Vector2 local)
        {
            _inMarket = true;
            _airBuffs ??= _player.GetComponent<BuffController>();
            bool onMarket = OverworldRules.SettlementAt(_layout, _origin + local) != null;
            bool nearPipe = TunnelStillAirRules.NearPipe(OverworldLayouts.TunnelAirPipes, local);
            bool bottle = _airBuffs != null && _airBuffs.HasEffectsFromSource(BottleSource);
            bool stifled = _stats.IsAlive && TunnelStillAirRules.Stifles(true, onMarket, nearPipe, false);
            ShowPipes(local);

            if (stifled && bottle)
            {
                SetStillAir(false);
                if (!_bottleSaid)
                {
                    _bottleSaid = true;
                    SayAir(RouteSightAirColdTexts.StillAirBottle);
                }

                return;
            }

            _bottleSaid = false;
            SetStillAir(stifled);
            if (!stifled)
            {
                return;
            }

            float drain = TunnelStillAirRules.DrainSeconds(_stats.CurrentStamina, _stats.MaxStamina, Time.deltaTime);
            if (drain > 0f)
            {
                _stats.DrainStamina(drain);
            }

            if (Time.unscaledTime >= _nextStillHint)
            {
                _nextStillHint = Time.unscaledTime + StillAirHintCooldown;
                SayAir(RouteSightAirColdTexts.StillAir);
            }
        }

        private void SetStillAir(bool on)
        {
            if (_airBuffs == null)
            {
                return;
            }

            if (!on)
            {
                if (_stillOn)
                {
                    _airBuffs.RemoveEffectsBySource(StillAirSource);
                    _stillOn = false;
                }

                return;
            }

            if (_stillOn && Time.time < _nextStillRefresh)
            {
                return;
            }

            _stillAirEffect ??= new StatModifierDefinition
            {
                displayName = "Ar parado",
                affectedStat = StatType.StaminaRegen,
                application = ModifierApplication.PercentAdditive,
                value = TunnelStillAirRules.StaminaRegenBonus,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            };
            _stillOn = true;
            _nextStillRefresh = Time.time + StillAirRefresh;
            _airBuffs.ApplyEffect(_stillAirEffect, StillAirSource);
        }

        private void ShowPipes(Vector2 local)
        {
            Vector2[] pipes = OverworldLayouts.TunnelAirPipes;
            _pipeLines ??= new LineRenderer[pipes.Length];
            for (int i = 0; i < pipes.Length; i++)
            {
                bool show = (pipes[i] - local).sqrMagnitude <= PipeViewRange * PipeViewRange;
                if (!show)
                {
                    if (_pipeLines[i] != null)
                    {
                        _pipeLines[i].enabled = false;
                    }

                    continue;
                }

                Post(ref _pipeLines[i], "Lm_TunnelAirPipe", 0.35f, pipes[i], PipeHeight, PipeColor);
            }
        }

        private void LeaveStillAir()
        {
            if (!_inMarket)
            {
                return;
            }

            _inMarket = false;
            _bottleSaid = false;
            SetStillAir(false);
            Hide(_pipeLines);
        }

        private void SayAir(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(RouteSightAirColdTexts.Get(localization, key));
        }
    }
}
