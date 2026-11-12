using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The waking stones of the Campo das Pedras-Dragão (21, <see cref="VishapStoneRules"/>), on the same driver as the boat and the hall: [E] at a
    /// sleeping vishapakar lights its spring (a column of water light; the hero heals through <c>PlayerStats.Heal</c> while near it) and wakes
    /// the nearest beast (ForceAggro plus short-lived effects on its BuffController, refreshed while it is awake, so they end by themselves if
    /// the driver stops). The day each stone was touched stays in the save (daily marks); the springs belong to this visit.
    /// </summary>
    public sealed partial class LimiarLakeMechanicsDriver
    {
        private const string AwakeSource = "vishap_awake";
        private const float StoneHintCooldown = 6f;
        private const float StoneViewRange = 120f;
        private const float StoneBeaconHeight = 7f;
        private const float AwakeRefresh = 0.5f;

        private static readonly Color StoneAsleepColor = new Color(0.5f, 0.85f, 0.95f, 0.85f);
        private static readonly Color SpringColor = new Color(0.45f, 0.95f, 1f, 1f);

        private LineRenderer[] _stoneLines;
        private float[] _springLit;
        private StatModifierDefinition _awakeDamage;
        private StatModifierDefinition _awakeSpeed;
        private EnemyController _awake;
        private float _awakeUntil;
        private float _nextAwakeRefresh;
        private float _nextStoneHint;
        private bool _inField;

        private void TickStones(Vector2 local)
        {
            _inField = true;
            Vector2[] stones = OverworldLayouts.VishapStones;
            if (_springLit == null)
            {
                _springLit = new float[stones.Length];
                for (int i = 0; i < _springLit.Length; i++)
                {
                    _springLit[i] = float.NegativeInfinity;
                }
            }

            ServiceLocator.TryGet(out WorldMapProgress progress);
            int day = ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;
            float now = Time.time;
            ShowStones(local, stones, progress, day, now);
            TickSprings(local, stones, now);
            TickAwake(now);

            int stone = VishapStoneRules.StoneAt(stones, local);
            if (stone < 0 || !_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            bool asleep = progress == null || VishapStoneRules.Asleep(progress.GetDailyMark(VishapStoneRules.TouchKey(stone)), day);
            if (!asleep)
            {
                if (Time.unscaledTime >= _nextStoneHint)
                {
                    _nextStoneHint = Time.unscaledTime + StoneHintCooldown;
                    SayStone(StonesFireGazeWindTexts.StoneSpent);
                }

                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextStoneHint)
            {
                _nextStoneHint = Time.unscaledTime + StoneHintCooldown;
                SayStone(StonesFireGazeWindTexts.StonePrompt);
            }

            if (_controller.Input.InteractPressedThisFrame)
            {
                Touch(stone, stones[stone], progress, day, now);
            }
        }

        private void Touch(int stone, Vector2 at, WorldMapProgress progress, int day, float now)
        {
            progress?.SetDailyMark(VishapStoneRules.TouchKey(stone), day);
            _springLit[stone] = now;
            _nextStoneHint = Time.unscaledTime + StoneHintCooldown;
            SayStone(StonesFireGazeWindTexts.StoneLit);

            EnemyController nearest = null;
            float best = float.MaxValue;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 p = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                float d = Vector2.Distance(p, at);
                if (VishapStoneRules.CanWake(enemy.IsBoss, d) && d < best)
                {
                    best = d;
                    nearest = enemy;
                }
            }

            if (nearest == null)
            {
                return;
            }

            ClearAwake();
            _awake = nearest;
            _awakeUntil = now + VishapStoneRules.AwakeSeconds;
            _nextAwakeRefresh = 0f;
            nearest.ForceAggro();
        }

        private void TickSprings(Vector2 local, Vector2[] stones, float now)
        {
            if (!_stats.IsAlive)
            {
                return;
            }

            for (int i = 0; i < stones.Length; i++)
            {
                if (!VishapStoneRules.Flowing(_springLit[i], now))
                {
                    continue;
                }

                float heal = VishapStoneRules.HealAmount(Vector2.Distance(stones[i], local), _stats.MaxHealth, Time.deltaTime);
                if (heal > 0f && _stats.CurrentHealth < _stats.MaxHealth)
                {
                    _stats.Heal(heal);
                }
            }
        }

        private void TickAwake(float now)
        {
            if (_awake == null)
            {
                return;
            }

            if (now >= _awakeUntil || _awake.Health == null || !_awake.Health.IsAlive || _awake.Buffs == null)
            {
                ClearAwake();
                return;
            }

            if (now < _nextAwakeRefresh)
            {
                return;
            }

            _nextAwakeRefresh = now + AwakeRefresh;
            _awakeDamage ??= NewEffect(StatType.AttackDamage, VishapStoneRules.AwakeDamageFactor);
            _awakeSpeed ??= NewEffect(StatType.MoveSpeed, VishapStoneRules.AwakeSpeedFactor);
            _awake.Buffs.ApplyEffect(_awakeDamage, AwakeSource);
            _awake.Buffs.ApplyEffect(_awakeSpeed, AwakeSource);
        }

        private void ClearAwake()
        {
            if (_awake != null && _awake.Buffs != null)
            {
                _awake.Buffs.RemoveEffectsBySource(AwakeSource);
            }

            _awake = null;
        }

        private void ShowStones(Vector2 local, Vector2[] stones, WorldMapProgress progress, int day, float now)
        {
            _stoneLines ??= new LineRenderer[stones.Length];
            for (int i = 0; i < stones.Length; i++)
            {
                bool flowing = VishapStoneRules.Flowing(_springLit[i], now);
                bool asleep = progress == null || VishapStoneRules.Asleep(progress.GetDailyMark(VishapStoneRules.TouchKey(i)), day);
                bool show = (flowing || asleep) && (stones[i] - local).sqrMagnitude <= StoneViewRange * StoneViewRange;
                if (!show)
                {
                    if (_stoneLines[i] != null)
                    {
                        _stoneLines[i].enabled = false;
                    }

                    continue;
                }

                Post(ref _stoneLines[i], "Lm_VishapSpring", flowing ? 1.2f : 0.25f, stones[i], flowing ? StoneBeaconHeight : 3f, flowing ? SpringColor : StoneAsleepColor);
            }
        }

        private void LeaveStones()
        {
            if (!_inField)
            {
                return;
            }

            _inField = false;
            Hide(_stoneLines);

            // The woken vishap stays awake out of the field only while this driver runs; its effects end by themselves otherwise.
            if (!isActiveAndEnabled)
            {
                ClearAwake();
            }
        }

        private void SayStone(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(StonesFireGazeWindTexts.Get(localization, key));
        }
    }
}
