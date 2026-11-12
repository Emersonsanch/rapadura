using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The low fire of the Savana dos Auroques (26, <see cref="SavannaFireRules"/>), on the same driver as the reef's tide: a short-lived speed
    /// effect on the hero on ash, and short-lived slow and soft effects on the beasts in a burnt or burning strip, all refreshed while they apply
    /// (they end by themselves if the driver stops). A line down the middle of each strip shows its state (orange fire, grey ash, nothing on old
    /// grass). Deals no damage and never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const string AshSource = "savanna_ash";
        private const string ShySource = "savanna_shy";
        private const float StripViewRange = 260f;
        private const float StripBeastRange = 80f;
        private const float StripBeastPass = 0.5f;
        private const int StripPoints = 9;

        private static readonly Color StripFireColor = new Color(1f, 0.45f, 0.1f, 1f);
        private static readonly Color StripAshColor = new Color(0.25f, 0.24f, 0.23f, 0.95f);

        private LineRenderer[] _stripLines;
        private SavannaStripState[] _stripDrawn;
        private StatModifierDefinition _ashSpeed;
        private StatModifierDefinition _shySpeed;
        private StatModifierDefinition _shyDamage;
        private bool _ashApplied;
        private float _nextAshRefresh;
        private float _nextStripBeastPass;
        private int _stripHere = -1;
        private bool _inSavanna;

        private void TickSavanna(Vector2 local, double now)
        {
            if (!OverworldLayouts.SavannaFireBounds.Contains(local))
            {
                LeaveSavanna();
                return;
            }

            _inSavanna = true;
            Rect[] strips = OverworldLayouts.SavannaFireStrips;
            bool inTown = OverworldRules.SettlementAt(_layout, _costaOrigin + local) != null;
            int strip = SavannaFireRules.StripAt(strips, local);
            SavannaStripState state = strip >= 0 ? SavannaFireRules.StateOf(strip, now) : SavannaStripState.Grass;
            bool fast = strip >= 0 && _stats.IsAlive && SavannaFireRules.HeroSpeedFactor(state, inTown) > 1f;
            SetAsh(fast);

            if (strip != _stripHere && strip >= 0 && !inTown && state != SavannaStripState.Grass)
            {
                SayRoute(SavannaFireRules.IsAsh(state) ? StonesFireGazeWindTexts.FireAsh : StonesFireGazeWindTexts.FireBurning);
            }

            _stripHere = strip;
            ShowStrips(local, strips, now);
            TickShyBeasts(local, strips, now);
        }

        private void SetAsh(bool on)
        {
            if (_buffs == null)
            {
                return;
            }

            if (!on)
            {
                if (_ashApplied)
                {
                    _buffs.RemoveEffectsBySource(AshSource);
                    _ashApplied = false;
                }

                return;
            }

            if (_ashApplied && Time.time < _nextAshRefresh)
            {
                return;
            }

            _ashSpeed ??= new StatModifierDefinition
            {
                displayName = "Cinza",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = SavannaFireRules.HeroAshFactor - 1f,
                durationSeconds = EffectSeconds,
                isDebuff = false,
            };
            _ashApplied = true;
            _nextAshRefresh = Time.time + EffectRefresh;
            _buffs.ApplyEffect(_ashSpeed, AshSource);
        }

        private void TickShyBeasts(Vector2 local, Rect[] strips, double now)
        {
            if (Time.time < _nextStripBeastPass)
            {
                return;
            }

            _nextStripBeastPass = Time.time + StripBeastPass;
            _shySpeed ??= NewSlow("Cinza", SavannaFireRules.BeastSpeedFactor - 1f);
            _shyDamage ??= new StatModifierDefinition
            {
                displayName = "Cinza",
                affectedStat = StatType.AttackDamage,
                application = ModifierApplication.PercentAdditive,
                value = SavannaFireRules.BeastDamageFactor - 1f,
                durationSeconds = EffectSeconds,
                isDebuff = true,
            };
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Buffs == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _costaOrigin;
                if ((at - local).sqrMagnitude > StripBeastRange * StripBeastRange)
                {
                    continue;
                }

                int strip = SavannaFireRules.StripAt(strips, at);
                if (strip >= 0 && SavannaFireRules.BeastShies(SavannaFireRules.StateOf(strip, now), enemy.IsBoss))
                {
                    enemy.Buffs.ApplyEffect(_shySpeed, ShySource);
                    enemy.Buffs.ApplyEffect(_shyDamage, ShySource);
                }
            }
        }

        private void ShowStrips(Vector2 local, Rect[] strips, double now)
        {
            _stripLines ??= new LineRenderer[strips.Length];
            _stripDrawn ??= new SavannaStripState[strips.Length];
            for (int i = 0; i < strips.Length; i++)
            {
                SavannaStripState state = SavannaFireRules.StateOf(i, now);
                Vector2 nearest = new Vector2(Mathf.Clamp(local.x, strips[i].xMin, strips[i].xMax), Mathf.Clamp(local.y, strips[i].yMin, strips[i].yMax));
                bool show = state != SavannaStripState.Grass && (nearest - local).sqrMagnitude <= StripViewRange * StripViewRange;
                LineRenderer line = _stripLines[i];
                if (!show)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }

                    continue;
                }

                if (line == null)
                {
                    line = _stripLines[i] = NewLine("Cs_SavannaStrip", 1f);
                    line.positionCount = StripPoints;
                    float x = strips[i].center.x;
                    for (int k = 0; k < StripPoints; k++)
                    {
                        float z = Mathf.Lerp(strips[i].yMin, strips[i].yMax, k / (float)(StripPoints - 1));
                        Vector2 world = _costaOrigin + new Vector2(x, z);
                        line.SetPosition(k, new Vector3(world.x, Mathf.Max(0f, Ground(world)) + 0.3f, world.y));
                    }

                    _stripDrawn[i] = SavannaStripState.Grass;
                }

                if (_stripDrawn[i] != state)
                {
                    _stripDrawn[i] = state;
                    bool fire = state == SavannaStripState.Burning;
                    line.startColor = line.endColor = fire ? StripFireColor : StripAshColor;
                    line.startWidth = line.endWidth = fire ? 2.4f : 1f;
                }

                line.enabled = true;
            }
        }

        private void LeaveSavanna()
        {
            if (!_inSavanna)
            {
                return;
            }

            _inSavanna = false;
            _stripHere = -1;
            SetAsh(false);
            if (_stripLines != null)
            {
                foreach (LineRenderer line in _stripLines)
                {
                    if (line != null)
                    {
                        line.enabled = false;
                    }
                }
            }
        }

        private void SayRoute(string key)
        {
            Rapadura.Core.DI.ServiceLocator.TryGet(out Rapadura.Core.Localization.LocalizationManager localization);
            SayText(StonesFireGazeWindTexts.Get(localization, key));
        }
    }
}
