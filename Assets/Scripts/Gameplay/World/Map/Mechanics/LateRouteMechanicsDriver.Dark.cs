using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The dark of the Cova da Treva and the Túneis Cegos (88, <see cref="DarkPitLightRules"/>): in the two zones, a few times a second, every beast
    /// near the hero out of every light gets a short-lived +30% Defense on its BuffController (refreshed while it stays dark, so it ends by itself
    /// when the driver stops) and a lit one loses it. Lit is: within the hero's light (Maria's wide one, else a carried lamp's), within a burning
    /// stand, round the Lâmpada 5 post, or for a few seconds after a Holy hit. The six stands are pillars on the roads (dark grey out, gold
    /// burning); [E] at one lights it for 10 min (one time mark, written only by the [E]). Bosses are left to their own fight.
    /// </summary>
    public sealed partial class LateRouteMechanicsDriver
    {
        private const int DarkLineKey = 500;
        private const int DarkSpot = 500;
        private const string DarkSource = "late_route_dark";
        private const float DarkEffectSeconds = 1.2f;
        private const float DarkPassSeconds = 0.25f;
        private const float DarkBeastRange = 60f;

        private static readonly Color StandOutColor = new Color(0.3f, 0.3f, 0.34f, 1f);
        private static readonly Color StandLitColor = new Color(1f, 0.82f, 0.4f, 1f);

        private static string[] s_standKeys;

        private readonly Dictionary<int, double> _holyUntil = new Dictionary<int, double>();
        private readonly HashSet<int> _darkBeasts = new HashSet<int>();
        private double[] _standLitUntil;
        private StatModifierDefinition _darkEffect;
        private bool _inDark;
        private float _nextDarkPass;
        private float _heroLight;

        /// <summary>True while the hero stands at a lamp stand.</summary>
        private bool TickDark(Vector2 local, double now)
        {
            if (!InDarkPit(local))
            {
                LeaveDark();
                return false;
            }

            Vector2[] stands = OverworldLayouts.DarkPitStands;
            if (!_inDark)
            {
                EnterDark(stands.Length);
            }

            MapRequirementState state = _heroState.Get(_bag);
            _heroLight = DarkPitLightRules.HeroLightRadius(IsMaria(), MapRequirementRules.HasLamp(state));
            for (int i = 0; i < stands.Length; i++)
            {
                bool burns = DarkPitLightRules.StandBurns(_standLitUntil[i], now);
                Pillar(DarkLineKey + i, stands[i] + _profundoOrigin, burns ? 5f : 3.5f, burns ? 0.7f : 0.5f, burns ? StandLitColor : StandOutColor);
            }

            if (Time.time >= _nextDarkPass)
            {
                _nextDarkPass = Time.time + DarkPassSeconds;
                TickDarkBeasts(local, stands, now);
            }

            if (!CanInteract)
            {
                return false;
            }

            int stand = DarkPitLightRules.StandAt(stands, local);
            if (stand < 0)
            {
                return false;
            }

            int spot = DarkSpot + stand;
            if (DarkPitLightRules.StandBurns(_standLitUntil[stand], now))
            {
                Hint(spot, Format(LateRouteMechanicsTexts.StandBurning, Mathf.CeilToInt((float)(_standLitUntil[stand] - now) / 60f)));
                return true;
            }

            HintKey(spot, LateRouteMechanicsTexts.StandPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!Pressed)
            {
                return true;
            }

            double until = DarkPitLightRules.LightStand(now);
            _standLitUntil[stand] = until;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(StandKeys(stands.Length)[stand], (long)Math.Ceiling(until));
            }

            Say(LateRouteMechanicsTexts.StandLit);
            _nextDarkPass = 0f;
            return true;
        }

        /// <summary>Whether a point (Profundo coordinates) is in one of the dark zones of map 88.</summary>
        public static bool InDarkPit(Vector2 local)
        {
            foreach (Rect area in OverworldLayouts.DarkPitAreas)
            {
                if (area.Contains(local))
                {
                    return true;
                }
            }

            return false;
        }

        private void EnterDark(int standCount)
        {
            _inDark = true;
            _standLitUntil ??= new double[standCount];
            ServiceLocator.TryGet(out WorldMapProgress progress);
            string[] keys = StandKeys(standCount);
            for (int i = 0; i < standCount; i++)
            {
                _standLitUntil[i] = progress != null ? progress.GetTimeMark(keys[i]) : 0d;
            }

            _darkEffect ??= new StatModifierDefinition
            {
                displayName = "Treva",
                affectedStat = StatType.Defense,
                application = ModifierApplication.PercentAdditive,
                value = DarkPitLightRules.DarkDefense,
                durationSeconds = DarkEffectSeconds,
                isDebuff = false,
            };

            _nextDarkPass = 0f;
            SayOnce(LateRouteMechanicsTexts.DarkEnter);
            if (IsMaria())
            {
                SayOnce(LateRouteMechanicsTexts.DarkMaria);
            }
        }

        /// <summary>Out of the dark (or the driver stops): the beasts lose the dark's Defense at once, the Holy glows are forgotten.</summary>
        private void LeaveDark()
        {
            if (!_inDark)
            {
                return;
            }

            _inDark = false;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && enemy.Buffs != null && _darkBeasts.Contains(enemy.GetInstanceID()))
                {
                    enemy.Buffs.RemoveEffectsBySource(DarkSource);
                }
            }

            _darkBeasts.Clear();
            _holyUntil.Clear();
        }

        private void TickDarkBeasts(Vector2 local, Vector2[] stands, double now)
        {
            Vector2 post = OverworldLayouts.LampadaTrevaOffset;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Buffs == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _profundoOrigin;
                if ((at - local).sqrMagnitude > DarkBeastRange * DarkBeastRange || !InDarkPit(at))
                {
                    continue;
                }

                int id = enemy.GetInstanceID();
                double holy = _holyUntil.TryGetValue(id, out double until) ? until : 0d;
                if (DarkPitLightRules.Lit(at, local, _heroLight, stands, _standLitUntil, post, holy, now))
                {
                    if (_darkBeasts.Remove(id))
                    {
                        enemy.Buffs.RemoveEffectsBySource(DarkSource);
                    }
                }
                else
                {
                    _darkBeasts.Add(id);
                    enemy.Buffs.ApplyEffect(_darkEffect, DarkSource);
                }
            }
        }

        /// <summary>The hero hit something: a Holy hit lights the beast for a few seconds; a blow into the dark says why it is weak.</summary>
        private void OnHeroHit(GameObject target, ElementType element)
        {
            if (!_inDark)
            {
                return;
            }

            EnemyController enemy = target.GetComponentInParent<EnemyController>();
            if (enemy == null)
            {
                return;
            }

            int id = enemy.GetInstanceID();
            if (element == ElementType.Holy)
            {
                double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
                _holyUntil[id] = now + DarkPitLightRules.HolyLitSeconds;
                if (_darkBeasts.Remove(id) && enemy.Buffs != null)
                {
                    enemy.Buffs.RemoveEffectsBySource(DarkSource);
                }
            }
            else if (_darkBeasts.Contains(id))
            {
                SayOnce(LateRouteMechanicsTexts.DarkHit);
            }
        }

        private static bool IsMaria() => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected == CharacterId.Maria;

        private static string[] StandKeys(int count)
        {
            if (s_standKeys == null || s_standKeys.Length != count)
            {
                var keys = new string[count];
                for (int i = 0; i < count; i++)
                {
                    keys[i] = DarkPitLightRules.StandKey(i);
                }

                s_standKeys = keys;
            }

            return s_standKeys;
        }
    }
}
