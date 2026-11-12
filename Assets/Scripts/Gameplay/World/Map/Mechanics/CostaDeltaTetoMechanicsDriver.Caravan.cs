using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The escort of Ammo's caravan through the Passo de Wakhan (40, <see cref="CaravanEscortRules"/>): three camels (columns) walk the caravan
    /// road while the hero keeps up; ambushes force the beasts round the bend onto the hero (<c>ForceAggro</c>) and avalanches mark a ring at
    /// the caravan's head (the hero caught in it loses health through <c>PlayerStats.ApplyHazardDamage</c>, which never kills). The driver stays
    /// on while the escort runs (<see cref="MechanicsBusy"/>); the reward's UTC day stays in the save. Never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const int CaravanCamels = 3;
        private const float CamelGap = 4f;
        private const float CamelHeight = 2.4f;
        private const int AvalancheKey = 700;
        private const float CaravanWaitHintCooldown = 8f;

        private static readonly Color CamelColor = new Color(0.78f, 0.6f, 0.38f, 1f);
        private static readonly Color AvalancheColor = new Color(0.95f, 0.98f, 1f, 1f);

        private readonly List<EnemyController> _ambushers = new List<EnemyController>();
        private readonly LineRenderer[] _camels = new LineRenderer[CaravanCamels];
        private bool _caravanOn;
        private float _caravanDistance;
        private float _caravanLength;
        private CaravanState _caravanState;
        private float _stopSeconds;
        private float _nextWaitHint;
        private bool _avalancheHit;

        private void TickCaravan(Vector2 local, double now)
        {
            if (!_caravanOn)
            {
                TickCaravanStart(local, now);
                return;
            }

            if (!_stats.IsAlive || !OverworldLayouts.InWakhanEscort(local) || !CaravanEscortRules.PassOpen(Daylight()))
            {
                AbandonCaravan();
                return;
            }

            Vector2[] road = OverworldLayouts.WakhanCaravanRoad;
            Vector2 head = RidePath.PointAt(road, _caravanDistance);
            float before = _caravanDistance / _caravanLength;
            switch (_caravanState)
            {
                case CaravanState.Ambush:
                    TickAmbush(head);
                    break;
                case CaravanState.Avalanche:
                    TickCaravanAvalanche(head, local);
                    break;
                default:
                    bool walks = CaravanEscortRules.Walks(Vector2.Distance(local, head), false);
                    _caravanState = walks ? CaravanState.Walking : CaravanState.Waiting;
                    if (!walks && Time.unscaledTime >= _nextWaitHint)
                    {
                        _nextWaitHint = Time.unscaledTime + CaravanWaitHintCooldown;
                        SayR4(CoastDeltaPassTexts.CaravanWaits);
                    }

                    if (walks)
                    {
                        _caravanDistance = Mathf.Min(_caravanLength, _caravanDistance + CaravanEscortRules.Speed * Time.deltaTime);
                        float after = _caravanDistance / _caravanLength;
                        if (CaravanEscortRules.StopCrossed(CaravanEscortRules.AmbushAt, before, after) >= 0)
                        {
                            StartAmbush(RidePath.PointAt(road, _caravanDistance));
                        }
                        else if (CaravanEscortRules.StopCrossed(CaravanEscortRules.AvalancheAt, before, after) >= 0)
                        {
                            _caravanState = CaravanState.Avalanche;
                            _stopSeconds = 0f;
                            _avalancheHit = false;
                            SayR4(CoastDeltaPassTexts.CaravanAvalanche);
                        }
                        else if (_caravanDistance >= _caravanLength)
                        {
                            FinishCaravan(now);
                            return;
                        }
                    }

                    break;
            }

            ShowCamels(road);
        }

        private void TickCaravanStart(Vector2 local, double now)
        {
            Vector2 start = OverworldLayouts.WakhanCaravanRoad[0];
            bool near = (start - local).sqrMagnitude <= 120f * 120f;
            if (!near)
            {
                Off(_camels);
                return;
            }

            _caravanDistance = 0f;
            ShowCamels(OverworldLayouts.WakhanCaravanRoad);
            if ((start - local).sqrMagnitude > CaravanEscortRules.StartReach * CaravanEscortRules.StartReach || !_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            long today = (long)System.Math.Floor(now / 86400d);
            ServiceLocator.TryGet(out WorldMapProgress progress);
            bool paid = progress != null && CaravanEscortRules.Paid(progress.GetDailyMark(CaravanEscortRules.RewardKey), today);
            bool open = CaravanEscortRules.PassOpen(Daylight());
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayR4(!open ? CoastDeltaPassTexts.CaravanNight : paid ? CoastDeltaPassTexts.CaravanPaid : CoastDeltaPassTexts.CaravanPrompt);
            }

            if (!open || paid)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                _caravanOn = true;
                _caravanDistance = 0f;
                _caravanLength = RidePath.Length(OverworldLayouts.WakhanCaravanRoad);
                _caravanState = CaravanState.Walking;
                SayR4(CoastDeltaPassTexts.CaravanStart);
            }
        }

        private void StartAmbush(Vector2 head)
        {
            _caravanState = CaravanState.Ambush;
            _stopSeconds = 0f;
            _ambushers.Clear();
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _tetoOrigin;
                if ((at - head).sqrMagnitude <= CaravanEscortRules.AmbushRange * CaravanEscortRules.AmbushRange)
                {
                    _ambushers.Add(enemy);
                    enemy.ForceAggro();
                }
            }

            SayR4(CoastDeltaPassTexts.CaravanAmbush);
        }

        private void TickAmbush(Vector2 head)
        {
            _stopSeconds += Time.deltaTime;
            int alive = 0;
            foreach (EnemyController enemy in _ambushers)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _tetoOrigin;
                if ((at - head).sqrMagnitude <= CaravanEscortRules.AmbushClearRange * CaravanEscortRules.AmbushClearRange)
                {
                    alive++;
                }
            }

            if (CaravanEscortRules.AmbushOver(alive, _stopSeconds))
            {
                _ambushers.Clear();
                _caravanState = CaravanState.Walking;
                SayR4(CoastDeltaPassTexts.CaravanAmbushOver);
            }
        }

        private void TickCaravanAvalanche(Vector2 head, Vector2 local)
        {
            _stopSeconds += Time.deltaTime;
            bool falling = _stopSeconds >= CaravanEscortRules.AvalancheWarning;
            DrawRing(AvalancheKey, _tetoOrigin, head, CaravanEscortRules.AvalancheRadius, falling ? AvalancheColor : LaneWarnColor, true, false);
            if (falling && !_avalancheHit)
            {
                _avalancheHit = true;
                float hurt = CaravanEscortRules.AvalancheHurt(Vector2.Distance(local, head), _stats.CurrentHealth, _stats.MaxHealth);
                if (hurt > 0f)
                {
                    _stats.ApplyHazardDamage(hurt);
                }
            }

            if (_stopSeconds >= CaravanEscortRules.AvalancheWarning + CaravanEscortRules.AvalancheSeconds)
            {
                HideRing(AvalancheKey);
                _caravanState = CaravanState.Walking;
            }
        }

        private void FinishCaravan(double now)
        {
            long today = (long)System.Math.Floor(now / 86400d);
            _caravanOn = false;
            _caravanState = CaravanState.Arrived;
            HideRing(AvalancheKey);
            Off(_camels);
            if (PayGold(CaravanEscortRules.Purse))
            {
                ServiceLocator.TryGet(out WorldMapProgress progress);
                progress?.SetDailyMark(CaravanEscortRules.RewardKey, (int)today);
                SayR4(CoastDeltaPassTexts.CaravanArrived, CaravanEscortRules.Purse);
            }
        }

        private void ShowCamels(Vector2[] road)
        {
            Vector2 dir = RidePath.DirectionAt(road, _caravanDistance);
            for (int i = 0; i < CaravanCamels; i++)
            {
                Vector2 at = RidePath.PointAt(road, Mathf.Max(0f, _caravanDistance - i * CamelGap));
                if (_caravanDistance - i * CamelGap < 0f)
                {
                    at -= dir * (i * CamelGap - _caravanDistance);
                }

                _camels[i] = Pillar(_camels[i], "Tt_CaravanCamel", _tetoOrigin + at, 1f, CamelHeight, CamelColor);
            }
        }

        /// <summary>Ends a running escort without pay (fell, left the pass, nightfall, the driver stopped).</summary>
        private void AbandonCaravan()
        {
            if (_caravanOn && _player != null && isActiveAndEnabled)
            {
                SayR4(CoastDeltaPassTexts.CaravanLost);
            }

            _caravanOn = false;
            _ambushers.Clear();
            HideRing(AvalancheKey);
            Off(_camels);
        }
    }
}
