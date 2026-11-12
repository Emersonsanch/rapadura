using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The deep stretches and the sluices of the Cova do Lodo (87, <see cref="ProfundoSluiceRules"/>).</summary>
    public sealed partial class ProfundoPathsMechanicsDriver
    {
        private const string WadeSource = "profundo_deep_water";
        private const float WadeEffectSeconds = 1.2f;
        private const float WadeEffectRefresh = 0.5f;
        private const float SluiceViewRange = 320f;
        private const float WaterLineLift = 0.25f;
        private const float LeverHeight = 6f;

        private static readonly Color DeepColor = new Color(0.2f, 0.45f, 0.55f, 0.95f);
        private static readonly Color LowColor = new Color(0.45f, 0.5f, 0.45f, 0.4f);
        private static readonly Color LeverColor = new Color(1f, 0.72f, 0.3f, 1f);

        private LineRenderer[] _sluiceLines;
        private LineRenderer[] _leverLines;
        private int[] _drainedLocalDay;
        private StatModifierDefinition _wadeSlow;
        private bool _wadeApplied;
        private float _nextWadeRefresh;
        private float _wading;
        private int _bitesApplied;
        private int _wadeStretch = -1;

        /// <summary>Whether sluice <paramref name="index"/> was opened on <paramref name="today"/> in this save.</summary>
        public static bool DrainedIn(WorldMapProgress progress, int index, int today) =>
            progress != null && ProfundoSluiceRules.DrainedToday(progress.GetDailyMark(SluiceKey(index)), today);

        /// <summary>The save key of sluice <paramref name="index"/>, built once (Drained is read every frame for every visible sluice).</summary>
        private static string SluiceKey(int index)
        {
            ProfundoSluice[] sluices = OverworldLayouts.ProfundoSluices;
            if (s_sluiceKeys == null || s_sluiceKeys.Length != sluices.Length)
            {
                var keys = new string[sluices.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = ProfundoSluiceRules.MarkKey(sluices[i].Id);
                }

                s_sluiceKeys = keys;
            }

            return s_sluiceKeys[index];
        }

        private static string[] s_sluiceKeys;

        private bool Drained(int index)
        {
            int today = Today();
            return ServiceLocator.TryGet(out WorldMapProgress progress) ? DrainedIn(progress, index, today) : ProfundoSluiceRules.DrainedToday(_drainedLocalDay[index], today);
        }

        /// <summary>Wading the deep water while it is up: slow, and once the bubble's breath runs out the black water bites.</summary>
        private void TickWade(Vector2 local)
        {
            int stretch = ProfundoSluiceRules.WaterAt(OverworldLayouts.ProfundoSluices, local);
            if (stretch < 0 || Drained(stretch))
            {
                ClearWade();
                return;
            }

            if (_wadeStretch != stretch)
            {
                _wadeStretch = stretch;
                _wading = 0f;
                _bitesApplied = 0;
                Say(ProfundoPathsTexts.WaterDeep);
            }

            _wading += Time.deltaTime;
            if (_buffs != null && Time.time >= _nextWadeRefresh)
            {
                _nextWadeRefresh = Time.time + WadeEffectRefresh;
                _buffs.ApplyEffect(_wadeSlow, WadeSource);
                _wadeApplied = true;
            }

            int due = ProfundoSluiceRules.BitesDue(_wading);
            if (_stats == null || due <= _bitesApplied)
            {
                return;
            }

            bool first = _bitesApplied == 0;
            _bitesApplied = due;
            float bite = ProfundoSluiceRules.BiteAmount(_stats.CurrentHealth, _stats.MaxHealth, IsNight());
            if (bite > 0f)
            {
                _stats.ApplyHazardDamage(bite);
                if (first)
                {
                    Say(ProfundoPathsTexts.WaterBites);
                }
            }
        }

        private void ClearWade()
        {
            if (_wadeApplied && _buffs != null)
            {
                _buffs.RemoveEffectsBySource(WadeSource);
            }

            _wadeApplied = false;
            _wadeStretch = -1;
            _wading = 0f;
            _bitesApplied = 0;
        }

        /// <summary>True while the hero stands at a sluice's lever.</summary>
        private bool TickLevers(Vector2 local)
        {
            int index = ProfundoSluiceRules.LeverAt(OverworldLayouts.ProfundoSluices, local);
            if (index < 0)
            {
                return false;
            }

            bool drained = Drained(index);
            int spot = 300 + index;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(drained ? ProfundoPathsTexts.LeverDone : ProfundoPathsTexts.LeverReady);
            }

            // An open sluice has nothing left to pull: the button stays off and the hint says why.
            if (drained)
            {
                return true;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                Drain(index);
            }

            return true;
        }

        private void Drain(int index)
        {
            ProfundoSluice sluice = OverworldLayouts.ProfundoSluices[index];
            int today = Today();
            _drainedLocalDay[index] = today;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetDailyMark(SluiceKey(index), today);
            }

            _hintSpot = -1;
            Say(ProfundoPathsTexts.Drained);
            if (Rush(sluice.Water))
            {
                Say(ProfundoPathsTexts.Rush);
            }
        }

        /// <summary>The rush of the draining water knocks the beasts standing in the stretch.</summary>
        private bool Rush(Rect localWater)
        {
            var world = new Rect(localWater.position + _origin, localWater.size);
            bool hit = false;
            foreach (Enemies.EnemyController enemy in Enemies.EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if (!world.Contains(new Vector2(p.x, p.z)))
                {
                    continue;
                }

                float damage = ProfundoSluiceRules.RushAmount(enemy.Health.MaxHealth, enemy.IsBoss);
                if (damage > 0f)
                {
                    enemy.Health.ApplyDamage(damage, ElementType.Physical, _player.gameObject);
                    hit = true;
                }
            }

            return hit;
        }

        private void ShowSluices(Vector2 local)
        {
            ProfundoSluice[] sluices = OverworldLayouts.ProfundoSluices;
            for (int i = 0; i < sluices.Length; i++)
            {
                Rect water = sluices[i].Water;
                bool visible = PartBounds[1].Contains(local) && Mathf.Abs(water.center.y - local.y) < SluiceViewRange;
                bool drained = visible && Drained(i);
                LineRenderer outline = _sluiceLines[i];
                LineRenderer lever = _leverLines[i];
                if (!visible)
                {
                    if (outline != null)
                    {
                        outline.enabled = false;
                    }

                    if (lever != null)
                    {
                        lever.enabled = false;
                    }

                    continue;
                }

                if (outline == null)
                {
                    outline = _sluiceLines[i] = NewLine("Pf_DeepWater", 0.6f, 5);
                    outline.loop = false;
                }

                // The edge of the deep water, on the near and the far side (the long sides of the band) and its two ends.
                float y = Mathf.Max(OverworldRelief.WaterY, _ground.GroundAt(_layout, _origin + new Vector2(local.x, water.center.y))) + WaterLineLift;
                outline.enabled = true;
                outline.startColor = outline.endColor = drained ? LowColor : DeepColor;
                outline.SetPosition(0, World(water.xMin, water.yMin, y));
                outline.SetPosition(1, World(water.xMax, water.yMin, y));
                outline.SetPosition(2, World(water.xMax, water.yMax, y));
                outline.SetPosition(3, World(water.xMin, water.yMax, y));
                outline.SetPosition(4, World(water.xMin, water.yMin, y));

                if (lever == null)
                {
                    lever = _leverLines[i] = NewLine("Pf_SluiceLever", 0.45f);
                }

                Vector3 foot = Ground(sluices[i].Lever);
                lever.enabled = true;
                lever.startColor = lever.endColor = drained ? LowColor : LeverColor;
                lever.SetPosition(0, foot);
                lever.SetPosition(1, foot + Vector3.up * (drained ? LeverHeight * 0.4f : LeverHeight));
            }
        }

        private Vector3 World(float x, float z, float y) => new Vector3(_origin.x + x, y, _origin.y + z);
    }
}
