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
    /// The five Fontes do Lodo of the Coração do Lodo (map 35; numbers in <see cref="LodoFountainRules"/>): the black mud round the Olho do Lodo
    /// bites (a hazard, never past its floor) and drags the hero toward the eye (<see cref="GroundedPush"/>); [E] by a fountain purifies it in
    /// 3 s, its light burns the beasts round it and it stays a safe zone for 2 min (<see cref="LodoFountainBoard"/>). With the five lit the mud
    /// is gone and <see cref="RegionBossDirector"/> lets the Lodo-que-Engole rise; the first five of a game day give a Pedra da Água Viva.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const int FountainKeys = 500;
        private const int SafeKeys = 510;
        private const int MudKey = 520;

        /// <summary>Beyond this distance from the eye the fountains are left alone (the night's mud plus a margin to see them from).</summary>
        private const float LodoRange = LodoFountainRules.MudBaseRadius * LodoFountainRules.MudNightGrowth + 120f;

        private const float FountainBeamHeight = 12f;
        private const float FountainLightRange = 16f;
        private const float FountainLightIntensity = 2.5f;

        /// <summary>The ficha's Água Viva, #4FC3B0.</summary>
        private static readonly Color LivingWaterColor = new Color(0.31f, 0.765f, 0.69f, 1f);

        /// <summary>The ficha's violet bubbles of the mud, #6D4C7D.</summary>
        private static readonly Color MireBubbleColor = new Color(0.427f, 0.298f, 0.49f, 0.9f);

        private readonly LineRenderer[] _fountainBeams = new LineRenderer[LodoFountainRules.Count];
        private readonly Light[] _fountainLights = new Light[LodoFountainRules.Count];

        private bool _inLodo;
        private int _purifying = -1;
        private float _purifyLeft;
        private bool _wasInMud;
        private bool _wasSafe;
        private int _lastLitCount = -1;
        private float _nextMudBite;
        private float _nextPulse;
        private int _lodoRewardDay;

        private static Vector2 LodoEye => OverworldLayouts.LodoArena;

        private void TickLodo(Vector2 local, bool redraw)
        {
            if (Vector2.Distance(local, LodoEye) > LodoRange)
            {
                if (_inLodo)
                {
                    LeaveLodo();
                }

                return;
            }

            _inLodo = true;
            Vector2[] fountains = OverworldLayouts.LodoFountains;
            float[] litUntil = LodoFountainBoard.LitUntil;
            float now = Time.time;
            int lit = LodoFountainBoard.LitCount(now);
            if (_lastLitCount >= 0 && lit < _lastLitCount)
            {
                Say(CostaDeltaTetoMechanicsTexts.FountainOut);
            }

            _lastLitCount = lit;
            bool alive = _stats.IsAlive;
            float mud = LodoFountainRules.MudRadius(lit, IsNight());
            bool safe = LodoFountainRules.InSafeZone(fountains, litUntil, now, local);
            bool inMud = alive && LodoFountainRules.InMud(LodoEye, mud, safe, local);
            if (inMud && !_wasInMud)
            {
                // Whoever walks in is told first: the first bite comes a tick later.
                _nextMudBite = now + LodoFountainRules.BiteTickSeconds;
                Hint(CostaDeltaTetoMechanicsTexts.LodoEnter);
            }

            if (safe && !_wasSafe && mud > 0f)
            {
                Hint(CostaDeltaTetoMechanicsTexts.FountainSafe);
            }

            _wasInMud = inMud;
            _wasSafe = safe;
            if (inMud)
            {
                if (now >= _nextMudBite)
                {
                    _nextMudBite = now + LodoFountainRules.BiteTickSeconds;
                    if (ApplyHazard(_stats, LodoFountainRules.BiteFraction, LodoFountainRules.BiteFloorFraction) > 0f)
                    {
                        Hint(CostaDeltaTetoMechanicsTexts.LodoBite);
                    }
                }

                // Purifying holds the hero's feet: the mud only drags those who are not at a fountain's work.
                if (_purifying < 0 && _character != null && _character.enabled)
                {
                    GroundedPush.Shove(_character, LodoFountainRules.Pull(LodoEye, local, Time.deltaTime));
                }
            }

            TickPurify(local, fountains, litUntil, now, lit, alive);
            TickFountainPulse(fountains, now);
            if (redraw)
            {
                ShowLodo(fountains, litUntil, now, mud);
            }
        }

        private void TickPurify(Vector2 local, Vector2[] fountains, float[] litUntil, float now, int lit, bool alive)
        {
            if (_purifying >= 0)
            {
                if (!alive || Vector2.Distance(local, fountains[_purifying]) > LodoFountainRules.PurifyReach)
                {
                    _purifying = -1;
                    Say(CostaDeltaTetoMechanicsTexts.FountainStopped);
                    return;
                }

                _purifyLeft -= Time.deltaTime;
                if (_purifyLeft <= 0f)
                {
                    FinishPurify(_purifying, fountains, now, lit);
                }

                return;
            }

            int fountain = alive ? LodoFountainRules.FountainAt(fountains, local) : -1;
            if (fountain < 0 || !LodoFountainRules.CanPurify(litUntil[fountain], now))
            {
                return;
            }

            Hint(LodoFountainRules.IsLit(litUntil[fountain], now) ? CostaDeltaTetoMechanicsTexts.FountainRenew : CostaDeltaTetoMechanicsTexts.FountainPrompt);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                _purifying = fountain;
                _purifyLeft = LodoFountainRules.PurifySeconds;
                Say(CostaDeltaTetoMechanicsTexts.FountainPurifying);
                _nextHint = Time.unscaledTime + HintCooldown;
            }
        }

        private void FinishPurify(int fountain, Vector2[] fountains, float now, int litBefore)
        {
            _purifying = -1;
            LodoFountainBoard.Light(fountain, now);
            int lit = LodoFountainBoard.LitCount(now);
            _lastLitCount = lit;
            StrikeBeastsAt(fountains[fountain], true);
            _nextPulse = now + LodoFountainRules.PulseSeconds;
            if (lit >= LodoFountainRules.Count)
            {
                Say(CostaDeltaTetoMechanicsTexts.FountainsAll);
            }
            else
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(CostaDeltaTetoMechanicsTexts.Format(localization, CostaDeltaTetoMechanicsTexts.FountainLit, lit.ToString()));
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            WorldMapProgress progress = Progress();
            int today = Today();
            int marked = progress != null ? progress.GetDailyMark(LodoFountainRules.RewardMarkKey) : _lodoRewardDay;
            if (LodoFountainRules.GivesReward(litBefore, lit, marked, today) && TryGive(LodoFountainRules.RewardItemId))
            {
                _lodoRewardDay = today;
                progress?.SetDailyMark(LodoFountainRules.RewardMarkKey, today);
            }
        }

        /// <summary>The lit fountains keep burning the beasts that wade into their safe zones (bosses are not touched).</summary>
        private void TickFountainPulse(Vector2[] fountains, float now)
        {
            if (now < _nextPulse)
            {
                return;
            }

            _nextPulse = now + LodoFountainRules.PulseSeconds;
            float[] litUntil = LodoFountainBoard.LitUntil;
            for (int i = 0; i < fountains.Length && i < litUntil.Length; i++)
            {
                if (LodoFountainRules.IsLit(litUntil[i], now))
                {
                    StrikeBeastsAt(fountains[i], false);
                }
            }
        }

        private void StrikeBeastsAt(Vector2 fountain, bool burst)
        {
            Vector2 center = _deltaOrigin + fountain;
            float radiusSq = LodoFountainRules.SafeRadius * LodoFountainRules.SafeRadius;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if ((new Vector2(p.x, p.z) - center).sqrMagnitude > radiusSq)
                {
                    continue;
                }

                float amount = LodoFountainRules.BeastStrike(enemy.Health.MaxHealth, enemy.IsBoss, burst);
                if (amount > 0f)
                {
                    enemy.Health.ApplyDamage(amount, ElementType.Holy, burst ? _player.gameObject : null);
                }
            }
        }

        private void ShowLodo(Vector2[] fountains, float[] litUntil, float now, float mud)
        {
            for (int i = 0; i < fountains.Length; i++)
            {
                bool isLit = LodoFountainRules.IsLit(litUntil[i], now);
                Color reachColor = i == _purifying ? SparkColor : MireBubbleColor;
                DrawRing(FountainKeys + i, _deltaOrigin, fountains[i], LodoFountainRules.PurifyReach, reachColor, !isLit || i == _purifying, true);
                DrawRing(SafeKeys + i, _deltaOrigin, fountains[i], LodoFountainRules.SafeRadius, LivingWaterColor, isLit, true);
                ShowFountainLight(i, fountains[i], isLit);
            }

            DrawRing(MudKey, _deltaOrigin, LodoEye, Mathf.Max(0.5f, mud), MireBubbleColor, mud > 0f, true);
        }

        /// <summary>Only the clean fountains give light (the ficha's night): a beam of Água Viva and a small light while lit.</summary>
        private void ShowFountainLight(int index, Vector2 fountain, bool show)
        {
            LineRenderer beam = _fountainBeams[index];
            if (beam == null)
            {
                if (!show)
                {
                    return;
                }

                beam = _fountainBeams[index] = NewLine("Mx_LodoFountainBeam", 0.45f);
                Vector2 world = _deltaOrigin + fountain;
                float ground = Ground(world);
                beam.SetPosition(0, new Vector3(world.x, ground + 0.2f, world.y));
                beam.SetPosition(1, new Vector3(world.x, ground + FountainBeamHeight, world.y));
                beam.startColor = LivingWaterColor;
                beam.endColor = new Color(LivingWaterColor.r, LivingWaterColor.g, LivingWaterColor.b, 0f);

                var go = new GameObject("Mx_LodoFountainLight");
                go.transform.SetParent(transform, false);
                go.transform.position = new Vector3(world.x, ground + 2.5f, world.y);
                Light light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = LivingWaterColor;
                light.range = FountainLightRange;
                light.intensity = FountainLightIntensity;
                light.shadows = LightShadows.None;
                _fountainLights[index] = light;
            }

            beam.enabled = show;
            if (_fountainLights[index] != null)
            {
                _fountainLights[index].enabled = show;
            }
        }

        private void LeaveLodo()
        {
            _inLodo = false;
            _purifying = -1;
            _wasInMud = false;
            _wasSafe = false;
            _lastLitCount = -1;
            Vector2[] fountains = OverworldLayouts.LodoFountains;
            for (int i = 0; i < fountains.Length; i++)
            {
                HideRing(FountainKeys + i);
                HideRing(SafeKeys + i);
                ShowFountainLight(i, fountains[i], false);
            }

            HideRing(MudKey);
        }

        private static bool IsNight() => ServiceLocator.TryGet(out TimeOfDayManager time) && time != null && time.Phase == TimeOfDayPhase.Night;

        private static int Today() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock != null ? clock.Day : 1;
    }
}
