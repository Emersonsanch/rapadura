using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Oásis de Qocho getting its water back (09, <see cref="QochoOasisRules"/>), on the same driver as the sandstorm: once the Root That
    /// Drank the Well is defeated (<see cref="DefeatedEnemyRegistry"/>), the shore of the oasis lake mends the hero's life and stamina out of
    /// combat; before that, Yardena says once per visit that the water is low. The cousin's line the first time it mends is kept in the save.
    /// </summary>
    public sealed partial class AreiasMechanicsDriver
    {
        private const float OasisTickSeconds = 0.5f;
        private const float OasisSayCooldown = 40f;
        private const string OasisThankedKey = "qocho_oasis_thanked";

        private readonly HeroCombatClock _oasisCombat = new HeroCombatClock();
        private bool _onShore;
        private bool _lowSaid;
        private float _oasisTick;
        private float _nextOasisSay;

        private void TickOasis(Vector2 local)
        {
            bool onShore = QochoOasisRules.OnShore(local);
            if (!onShore)
            {
                LeaveOasis();
                return;
            }

            _oasisCombat.Track(_player.gameObject);
            bool freed = ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(QochoOasisRules.RootEnemyId);
            if (!_onShore && !freed && !_lowSaid)
            {
                _lowSaid = true;
                SayOasis(QochoOasisTexts.Low);
            }

            _onShore = true;
            if (!freed || !_stats.IsAlive)
            {
                return;
            }

            bool hurt = _stats.CurrentHealth < _stats.MaxHealth || _stats.CurrentStamina < _stats.MaxStamina;
            if (!QochoOasisRules.Mends(true, true, true, _oasisCombat.SecondsSinceCombat))
            {
                _oasisTick = 0f;
                if (hurt && Time.unscaledTime >= _nextOasisSay)
                {
                    _nextOasisSay = Time.unscaledTime + OasisSayCooldown;
                    SayOasis(QochoOasisTexts.Waits);
                }

                return;
            }

            _oasisTick += Time.deltaTime;
            if (_oasisTick < OasisTickSeconds)
            {
                return;
            }

            float seconds = _oasisTick;
            _oasisTick = 0f;
            if (!hurt)
            {
                return;
            }

            if (Time.unscaledTime >= _nextOasisSay)
            {
                _nextOasisSay = Time.unscaledTime + OasisSayCooldown;
                SayOasis(QochoOasisTexts.Mends);
                if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(OasisThankedKey) <= 0L)
                {
                    progress.SetTimeMark(OasisThankedKey, 1L);
                    SayOasis(QochoOasisTexts.LineFor(MapRequirementHeroState.Capture(null).Hero), -0.5f);
                }
            }

            float heal = QochoOasisRules.MendOver(_stats.CurrentHealth, _stats.MaxHealth, QochoOasisRules.HealPerSecond, seconds);
            if (heal > 0f)
            {
                _stats.Heal(heal);
            }

            float stamina = QochoOasisRules.MendOver(_stats.CurrentStamina, _stats.MaxStamina, QochoOasisRules.StaminaPerSecond, seconds);
            if (stamina > 0f)
            {
                _stats.RestoreStamina(stamina);
            }
        }

        private void LeaveOasis()
        {
            if (_onShore)
            {
                _lowSaid = false;
            }

            _onShore = false;
            _oasisTick = 0f;
        }

        private void SayOasis(string key, float drop = 0f)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), QochoOasisTexts.Get(localization, key));
        }
    }
}
