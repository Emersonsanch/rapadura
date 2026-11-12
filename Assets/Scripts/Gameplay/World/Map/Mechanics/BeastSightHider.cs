using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// When the hero cannot see far (the salt of Namrael at noon, the sandstorm of the Estrada das Caravanas, the fog of Garajonay), whether a
    /// beast is lost to sight, pure: only while the sight is cut, never a boss, and only past the range.
    /// </summary>
    public static class BeastSightRules
    {
        /// <summary>No cut: every beast shows.</summary>
        public const float NoLimit = 0f;

        /// <summary>Whether the sight is cut at all (a positive range).</summary>
        public static bool Active(float range) => range > 0f;

        /// <summary>Whether a beast <paramref name="distance"/> metres from the hero is lost while the sight reaches <paramref name="range"/> metres.</summary>
        public static bool Hidden(float range, bool isBoss, float distance) => Active(range) && !isBoss && distance > range;

        /// <summary>The shorter of two cuts (a non-positive range is no cut).</summary>
        public static float Shortest(float a, float b)
        {
            if (!Active(a))
            {
                return Active(b) ? b : NoLimit;
            }

            return Active(b) ? Mathf.Min(a, b) : a;
        }
    }

    /// <summary>
    /// Scene side of <see cref="BeastSightRules"/>, shared by the drivers that cut the hero's sight: five times a second it switches off every
    /// renderer under each beast lost to sight (and, on later passes, whatever turned up under it since: a night variant's eyes, a stun or buff
    /// aura parented to it, a renderer something else switched back on) and switches them back on as soon as the beast is seen again (it came
    /// close, died, despawned), when the cut ends, and in <see cref="RevealAll"/>, which each driver calls from OnDisable and on leaving the
    /// place. The beasts carry no world-space health bar or nameplate: the only world text, the damage numbers and the warnings, is spawned
    /// loose at the hit, not under the beast, so there is nothing else to hide.
    /// </summary>
    public sealed class BeastSightHider
    {
        public const float PassSeconds = 0.2f;

        private readonly Dictionary<EnemyController, List<Renderer>> _hidden = new Dictionary<EnemyController, List<Renderer>>();
        private readonly List<EnemyController> _seen = new List<EnemyController>();
        private readonly List<EnemyController> _stale = new List<EnemyController>();
        private readonly List<Renderer> _scratch = new List<Renderer>();
        private float _nextPass;

        /// <summary>How many beasts are hidden now.</summary>
        public int HiddenCount => _hidden.Count;

        /// <summary>One frame: with <paramref name="range"/> ≤ 0 every beast shows at once; otherwise a pass every <see cref="PassSeconds"/>.</summary>
        public void Tick(Vector3 hero, float range, float now)
        {
            if (!BeastSightRules.Active(range))
            {
                RevealAll();
                return;
            }

            if (now < _nextPass)
            {
                return;
            }

            _nextPass = now + PassSeconds;
            _seen.Clear();
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                float distance = new Vector2(p.x - hero.x, p.z - hero.z).magnitude;
                if (!BeastSightRules.Hidden(range, enemy.IsBoss, distance))
                {
                    continue;
                }

                _seen.Add(enemy);
                Hide(enemy);
            }

            _stale.Clear();
            foreach (EnemyController enemy in _hidden.Keys)
            {
                if (enemy == null || !_seen.Contains(enemy))
                {
                    _stale.Add(enemy);
                }
            }

            foreach (EnemyController enemy in _stale)
            {
                Reveal(enemy);
            }
        }

        public void RevealAll()
        {
            if (_hidden.Count == 0)
            {
                return;
            }

            foreach (List<Renderer> renderers in _hidden.Values)
            {
                Show(renderers);
            }

            _hidden.Clear();
            _nextPass = 0f;
        }

        private void Hide(EnemyController enemy)
        {
            if (!_hidden.TryGetValue(enemy, out List<Renderer> hidden))
            {
                hidden = new List<Renderer>();
                _hidden[enemy] = hidden;
            }

            _scratch.Clear();
            enemy.GetComponentsInChildren(false, _scratch);
            foreach (Renderer renderer in _scratch)
            {
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    if (!hidden.Contains(renderer))
                    {
                        hidden.Add(renderer);
                    }
                }
            }
        }

        private void Reveal(EnemyController enemy)
        {
            if (_hidden.TryGetValue(enemy, out List<Renderer> renderers))
            {
                _hidden.Remove(enemy);
                Show(renderers);
            }
        }

        private static void Show(List<Renderer> renderers)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }
    }
}
