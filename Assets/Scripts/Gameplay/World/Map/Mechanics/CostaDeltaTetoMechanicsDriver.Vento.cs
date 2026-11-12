using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The wind vanes of the Forte dos Ventos (39, <see cref="WindVaneRules"/>), on the same driver as the chutes' avalanches: [E] at a vane turns
    /// it a quarter (in the save: time marks), and in the gorge and the fort a short-lived speed effect follows the hero's walk with or against
    /// the wind, refreshed while it applies (it ends by itself if the driver stops). Each vane is a pole with an arm pointing where it blows.
    /// Deals no damage and never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const string WindSource = "teto_vane_wind";
        private const float VaneViewRange = 160f;
        private const float VaneHeight = 6f;
        private const float VaneArm = 2.5f;
        private const float VaneHintCooldown = 5f;
        private const float WindHintCooldown = 12f;

        private static readonly Color VanePoleColor = new Color(0.55f, 0.5f, 0.45f, 1f);
        private static readonly Color VaneArmColor = new Color(0.95f, 0.85f, 0.35f, 1f);

        private readonly int[] _vaneStates = new int[WindVaneRules.VaneCount];
        private LineRenderer[] _vanePoles;
        private LineRenderer[] _vaneArms;
        private bool _vanesLoaded;
        private bool _inFort;
        private float _windApplied = 1f;
        private float _nextWindRefresh;
        private float _nextVaneHint;
        private float _nextWindHint;
        private int _windSide;

        private void TickVanes(Vector2 local)
        {
            if (!OverworldLayouts.WindFortBounds.Contains(local))
            {
                LeaveVanes();
                return;
            }

            _inFort = true;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            LoadVanes(progress);
            Vector2[] vanes = OverworldLayouts.WindVanes;
            ShowVanes(local, vanes);

            int vane = WindVaneRules.VaneAt(vanes, local);
            if (vane >= 0 && _stats.IsAlive && _controller.Input != null)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (Time.unscaledTime >= _nextVaneHint)
                {
                    _nextVaneHint = Time.unscaledTime + VaneHintCooldown;
                    SayRoute(StonesFireGazeWindTexts.VanePrompt);
                }

                if (_controller.Input.InteractPressedThisFrame)
                {
                    TurnVane(vane, progress);
                }
            }

            Vector2 wind = WindVaneRules.Wind(_vaneStates);
            bool inFortGround = OverworldRules.SettlementAt(_layout, _tetoOrigin + local) != null;
            Vector3 v = _character != null && _character.enabled ? _character.velocity : Vector3.zero;
            float factor = _stats.IsAlive && !inFortGround ? WindVaneRules.SpeedFactor(new Vector2(v.x, v.z), wind) : 1f;
            SetWind(factor);
        }

        private void LoadVanes(WorldMapProgress progress)
        {
            if (_vanesLoaded)
            {
                return;
            }

            _vanesLoaded = true;
            for (int i = 0; i < _vaneStates.Length; i++)
            {
                _vaneStates[i] = WindVaneRules.StateFrom(progress != null ? progress.GetTimeMark(WindVaneRules.StateKey(i)) : 0L, i);
            }
        }

        private void TurnVane(int vane, WorldMapProgress progress)
        {
            _vaneStates[vane] = WindVaneRules.Turn(_vaneStates[vane]);
            progress?.SetTimeMark(WindVaneRules.StateKey(vane), WindVaneRules.MarkOf(_vaneStates[vane]));
            _nextVaneHint = Time.unscaledTime + VaneHintCooldown;
            bool full = WindVaneRules.Wind(_vaneStates).magnitude > 0.99f;
            SayRoute(full ? StonesFireGazeWindTexts.WindFull : StonesFireGazeWindTexts.VaneTurned);
            DrawVaneArm(vane, OverworldLayouts.WindVanes[vane]);
        }

        private void SetWind(float factor)
        {
            if (_buffs == null)
            {
                return;
            }

            int side = factor > 1.001f ? 1 : factor < 0.999f ? -1 : 0;
            if (side != _windSide && side != 0 && Time.unscaledTime >= _nextWindHint)
            {
                _nextWindHint = Time.unscaledTime + WindHintCooldown;
                SayRoute(side > 0 ? StonesFireGazeWindTexts.WindWith : StonesFireGazeWindTexts.WindAgainst);
            }

            _windSide = side;
            if (side == 0)
            {
                if (_windApplied != 1f)
                {
                    _buffs.RemoveEffectsBySource(WindSource);
                    _windApplied = 1f;
                }

                return;
            }

            if (Mathf.Abs(factor - _windApplied) < 0.001f && Time.time < _nextWindRefresh)
            {
                return;
            }

            if (_windApplied != 1f && (factor > 1f) != (_windApplied > 1f))
            {
                _buffs.RemoveEffectsBySource(WindSource);
            }

            _windApplied = factor;
            _nextWindRefresh = Time.time + EffectRefresh;
            _buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Vento",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = factor - 1f,
                durationSeconds = EffectSeconds,
                isDebuff = factor < 1f,
            }, WindSource);
        }

        private void ShowVanes(Vector2 local, Vector2[] vanes)
        {
            if (_vanePoles == null)
            {
                _vanePoles = new LineRenderer[vanes.Length];
                _vaneArms = new LineRenderer[vanes.Length];
            }

            for (int i = 0; i < vanes.Length; i++)
            {
                bool show = (vanes[i] - local).sqrMagnitude <= VaneViewRange * VaneViewRange;
                if (_vanePoles[i] == null)
                {
                    if (!show)
                    {
                        continue;
                    }

                    _vanePoles[i] = NewLine("Tt_WindVanePole", 0.25f);
                    _vanePoles[i].startColor = _vanePoles[i].endColor = VanePoleColor;
                    Vector3 foot = VaneFoot(vanes[i]);
                    _vanePoles[i].SetPosition(0, foot);
                    _vanePoles[i].SetPosition(1, foot + Vector3.up * VaneHeight);
                    _vaneArms[i] = NewLine("Tt_WindVaneArm", 0.35f);
                    _vaneArms[i].startColor = _vaneArms[i].endColor = VaneArmColor;
                    DrawVaneArm(i, vanes[i]);
                }

                _vanePoles[i].enabled = show;
                _vaneArms[i].enabled = show;
            }
        }

        private void DrawVaneArm(int vane, Vector2 at)
        {
            if (_vaneArms == null || _vaneArms[vane] == null)
            {
                return;
            }

            Vector2 d = WindVaneRules.Direction(_vaneStates[vane]);
            Vector3 top = VaneFoot(at) + Vector3.up * VaneHeight;
            _vaneArms[vane].SetPosition(0, top - new Vector3(d.x, 0f, d.y) * (VaneArm * 0.4f));
            _vaneArms[vane].SetPosition(1, top + new Vector3(d.x, 0f, d.y) * VaneArm);
        }

        private Vector3 VaneFoot(Vector2 local)
        {
            Vector2 world = _tetoOrigin + local;
            return new Vector3(world.x, Mathf.Max(0f, Ground(world)), world.y);
        }

        private void LeaveVanes()
        {
            if (!_inFort)
            {
                return;
            }

            _inFort = false;
            _windSide = 0;
            SetWind(1f);
            if (_vanePoles == null)
            {
                return;
            }

            for (int i = 0; i < _vanePoles.Length; i++)
            {
                if (_vanePoles[i] != null)
                {
                    _vanePoles[i].enabled = false;
                    _vaneArms[i].enabled = false;
                }
            }
        }
    }
}
