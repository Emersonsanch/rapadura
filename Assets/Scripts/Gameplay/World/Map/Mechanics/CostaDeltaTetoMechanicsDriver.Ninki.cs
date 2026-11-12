using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The calves' glow of the Charco do Ninki Nanka (34, <see cref="NinkiGazeRules"/>), on the same driver as the Delta's islands: each calf near
    /// the hero shines on its own beat (a golden halo over it), and a hero who faces one while it glows gets a 3 s slow on the BuffController,
    /// once per glow. The beat comes from the calf's instance, so each one shines at its own moment. Deals no damage and never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const string GazeSource = "ninki_gaze";
        private const float GazeScanRange = 30f;
        private const float GazePass = 0.1f;
        private const float GazeWarnCooldown = 6f;
        private const int GazeHalos = 6;

        private static readonly Color HaloWarnColor = new Color(1f, 0.85f, 0.4f, 0.6f);
        private static readonly Color HaloGlowColor = new Color(1f, 0.95f, 0.6f, 1f);

        private readonly Dictionary<int, long> _gazeHitGlow = new Dictionary<int, long>();
        private readonly LineRenderer[] _halos = new LineRenderer[GazeHalos];
        private StatModifierDefinition _gazeSlow;
        private float _nextGazePass;
        private float _nextGazeWarn;
        private bool _inBog;

        private void TickGaze(Vector2 local, double now)
        {
            if (!OverworldLayouts.NinkiBogBounds.Contains(local))
            {
                LeaveGaze();
                return;
            }

            _inBog = true;
            if (Time.time < _nextGazePass)
            {
                return;
            }

            _nextGazePass = Time.time + GazePass;
            Vector2 heroForward = new Vector2(_player.forward.x, _player.forward.z);
            int halo = 0;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Definition == null || enemy.Health == null || !enemy.Health.IsAlive
                    || enemy.Definition.name != OverworldLayouts.NinkiCalfEnemyId)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                Vector2 at = new Vector2(p.x, p.z) - _deltaOrigin;
                if ((at - local).sqrMagnitude > GazeScanRange * GazeScanRange || !OverworldLayouts.NinkiBogBounds.Contains(at))
                {
                    continue;
                }

                int id = enemy.GetInstanceID();
                float offset = NinkiGazeRules.Offset(id);
                NinkiGlowPhase phase = NinkiGazeRules.PhaseAt(now, offset);
                if (phase == NinkiGlowPhase.Dim)
                {
                    continue;
                }

                if (halo < GazeHalos)
                {
                    ShowHalo(halo++, p, phase == NinkiGlowPhase.Glowing);
                }

                bool near = (at - local).sqrMagnitude <= NinkiGazeRules.Reach * NinkiGazeRules.Reach;
                if (phase == NinkiGlowPhase.Warning)
                {
                    if (near && Time.unscaledTime >= _nextGazeWarn)
                    {
                        _nextGazeWarn = Time.unscaledTime + GazeWarnCooldown;
                        SayRoute(StonesFireGazeWindTexts.GazeWarning);
                    }

                    continue;
                }

                long glow = NinkiGazeRules.GlowIndex(now, offset);
                if (!_stats.IsAlive || (_gazeHitGlow.TryGetValue(id, out long last) && last == glow)
                    || !NinkiGazeRules.Facing(local, heroForward, at))
                {
                    continue;
                }

                _gazeHitGlow[id] = glow;
                Shame();
            }

            for (int i = halo; i < GazeHalos; i++)
            {
                if (_halos[i] != null)
                {
                    _halos[i].enabled = false;
                }
            }
        }

        private void Shame()
        {
            if (_buffs == null)
            {
                return;
            }

            _gazeSlow ??= new StatModifierDefinition
            {
                displayName = "Vergonha",
                affectedStat = StatType.MoveSpeed,
                application = ModifierApplication.PercentAdditive,
                value = NinkiGazeRules.SlowBonus,
                durationSeconds = NinkiGazeRules.SlowSeconds,
                isDebuff = true,
            };
            _buffs.ApplyEffect(_gazeSlow, GazeSource);
            SayRoute(StonesFireGazeWindTexts.GazeHit);
        }

        private void ShowHalo(int index, Vector3 calf, bool glowing)
        {
            LineRenderer line = _halos[index] != null ? _halos[index] : (_halos[index] = NewLine("Dt_NinkiHalo", 0.6f));
            line.enabled = true;
            line.startColor = line.endColor = glowing ? HaloGlowColor : HaloWarnColor;
            line.startWidth = line.endWidth = glowing ? 1.4f : 0.6f;
            line.SetPosition(0, calf + Vector3.up * 1.2f);
            line.SetPosition(1, calf + Vector3.up * (glowing ? 5f : 3f));
        }

        private void LeaveGaze()
        {
            if (!_inBog)
            {
                return;
            }

            _inBog = false;
            _gazeHitGlow.Clear();
            foreach (LineRenderer line in _halos)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }
    }
}
