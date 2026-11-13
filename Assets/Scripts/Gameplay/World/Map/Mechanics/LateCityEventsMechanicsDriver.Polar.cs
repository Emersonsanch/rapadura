using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Noite Polar of the Porto da Lua Branca (59, <see cref="PortoPolarNightRules"/>): three moon lanterns, the Creeping Dark that drinks them.</summary>
    public sealed partial class LateCityEventsMechanicsDriver
    {
        private const float GroundProbeHeight = 6f;
        private const float CorpseSeconds = 5f;
        private const float PolarPlaza = 10f;
        private const float PolarSoonEvery = 60f;
        private const float DrinkSayEvery = 4f;
        private const float LanternBeamHeight = 5f;
        private const string PolarAreaId = "porto_da_lua_branca";

        private static readonly Color LanternLitColor = new Color(0.82f, 0.86f, 1f, 0.95f);
        private static readonly Color LanternLowColor = new Color(0.45f, 0.4f, 0.7f, 0.85f);
        private static readonly Color LanternOutColor = new Color(0.12f, 0.12f, 0.2f, 0.7f);

        private readonly float[] _lights = new float[PortoPolarNightRules.LanternCount];
        private readonly bool[] _oilUsed = new bool[PortoPolarNightRules.LanternCount];
        private readonly bool[] _outSaid = new bool[PortoPolarNightRules.LanternCount];
        private readonly int[] _drinkers = new int[PortoPolarNightRules.LanternCount];
        private readonly List<GameObject> _darks = new List<GameObject>();
        private readonly List<KeyValuePair<GameObject, float>> _corpses = new List<KeyValuePair<GameObject, float>>();
        private GameObject _prefab;
        private EnemyDefinition _darkDefinition;
        private Transform[] _lanternTargets;
        private LineRenderer[] _lanternBeams;

        private bool _polarRunning;
        private int _polarRunDay;
        private int _polarLightsDay = int.MinValue;
        private int _darkWaves;
        private int _polarWarnedDay = int.MinValue;
        private long _polarPaidLocal;
        private int _polarOwedDay = int.MinValue;
        private int _polarOwedLit;
        private float _nextPolarOwedTry;
        private float _nextPolarSoon;
        private float _nextDrinkSay;
        private float _nextPolarHint;

        private void TickPolar(Vector2 local)
        {
            ReleaseCorpses();
            float distance = Vector2.Distance(local, OverworldLayouts.PortoLuaBrancaOffset);
            if (!ServiceLocator.TryGet(out TimeOfDayManager clock) || clock == null)
            {
                StopPolar();
                return;
            }

            if (_polarOwedDay != int.MinValue && distance <= PortoPolarNightRules.LeaveRange && Time.unscaledTime >= _nextPolarOwedTry)
            {
                PayPolar(_polarOwedDay, _polarOwedLit);
            }

            int day = clock.Day;
            float hour = clock.CurrentHour;
            if (!_polarRunning)
            {
                HidePolar();
                if (distance > PortoPolarNightRules.AlertRange)
                {
                    return;
                }

                ClosedPolar(day, hour, distance);
                long paid = GetMark(PortoPolarNightRules.PaidKey, _polarPaidLocal);
                if (PortoPolarNightRules.CanStart(day, hour, paid, distance) && _polarOwedDay != day)
                {
                    StartPolar(day, hour);
                }

                return;
            }

            // A defence under way ends at the dawn (15 h) or when abandoned; it is never cut in the middle by anything else.
            if (distance > PortoPolarNightRules.LeaveRange)
            {
                Say(LateCityEventsTexts.PolarAbandoned);
                StopPolar();
                return;
            }

            if (PortoPolarNightRules.Resolve(day, hour, _polarRunDay) == PolarNightOutcome.Dawn)
            {
                int runDay = _polarRunDay;
                int lit = PortoPolarNightRules.LitCount(_lights);
                StopPolar();
                Say(LateCityEventsTexts.PolarDawn);
                PayPolar(runDay, lit);
                return;
            }

            _darks.RemoveAll(dark => dark == null);
            if (PortoPolarNightRules.ShouldSpawnWave(_darkWaves, hour, _darks.Count) && SpawnDarkWave(_darkWaves))
            {
                _darkWaves++;
                SayFormat(LateCityEventsTexts.PolarWave, _darkWaves, PortoPolarNightRules.WaveCount);
            }

            DrainLanterns();
            if (_stats.IsAlive)
            {
                TickRelight(local);
            }

            ShowPolar();
        }

        private void ClosedPolar(int day, float hour, float distance)
        {
            if (PortoPolarNightRules.IsWarning(day, hour))
            {
                if (_polarWarnedDay != day)
                {
                    _polarWarnedDay = day;
                    Say(LateCityEventsTexts.PolarWarning);
                }

                return;
            }

            if (distance > PolarPlaza || Time.unscaledTime < _nextPolarSoon || PortoPolarNightRules.IsOpen(day, hour))
            {
                return;
            }

            _nextPolarSoon = Time.unscaledTime + PolarSoonEvery;
            int wait = PortoPolarNightRules.DaysUntilPolar(day);
            if (wait == 0 && hour < PortoPolarNightRules.StartHour)
            {
                Say(LateCityEventsTexts.PolarToday);
            }
            else if (wait > 0)
            {
                SayFormat(LateCityEventsTexts.PolarSoon, wait);
            }
        }

        private void StartPolar(int day, float hour)
        {
            if (!EnsureDarkSpawning())
            {
                return;
            }

            if (_polarLightsDay != day)
            {
                // A new dark day: full lanterns and fresh oil. Within the same day they keep what they lost (leaving and coming back resets nothing).
                _polarLightsDay = day;
                for (int i = 0; i < PortoPolarNightRules.LanternCount; i++)
                {
                    _lights[i] = PortoPolarNightRules.LightMax;
                    _oilUsed[i] = false;
                    _outSaid[i] = false;
                }
            }

            _polarRunning = true;
            _polarRunDay = day;
            _darkWaves = PortoPolarNightRules.FirstWave(hour);
            Say(LateCityEventsTexts.PolarStart);
        }

        /// <summary>Ends the defence: the living Dark goes back into the ice (the dead keep their few seconds of corpse).</summary>
        private void StopPolar()
        {
            foreach (GameObject dark in _darks)
            {
                if (dark != null)
                {
                    Destroy(dark);
                }
            }

            _darks.Clear();
            _polarRunning = false;
            HidePolar();
        }

        private void DrainLanterns()
        {
            Vector2[] lanterns = OverworldLayouts.PortoPolarLanterns;
            for (int i = 0; i < lanterns.Length && i < _lights.Length; i++)
            {
                _drinkers[i] = 0;
            }

            foreach (GameObject dark in _darks)
            {
                Vector3 p = dark.transform.position;
                Vector2 at = new Vector2(p.x, p.z) - _portoOrigin;
                for (int i = 0; i < lanterns.Length && i < _lights.Length; i++)
                {
                    if (PortoPolarNightRules.Drinking(lanterns[i], at))
                    {
                        _drinkers[i]++;
                    }
                }
            }

            for (int i = 0; i < lanterns.Length && i < _lights.Length; i++)
            {
                bool wasOut = PortoPolarNightRules.IsOut(_lights[i]);
                _lights[i] = PortoPolarNightRules.Drain(_lights[i], _drinkers[i], Time.deltaTime);
                if (!wasOut && PortoPolarNightRules.IsOut(_lights[i]) && !_outSaid[i])
                {
                    _outSaid[i] = true;
                    SayFormat(LateCityEventsTexts.PolarOut, SideName(i));
                }
                else if (_drinkers[i] > 0 && !PortoPolarNightRules.IsOut(_lights[i]) && Time.unscaledTime >= _nextDrinkSay)
                {
                    _nextDrinkSay = Time.unscaledTime + DrinkSayEvery;
                    SayFormat(LateCityEventsTexts.PolarDrinking, SideName(i), Mathf.CeilToInt(_lights[i] / PortoPolarNightRules.LightMax * 100f));
                }
            }
        }

        private void TickRelight(Vector2 local)
        {
            int lantern = MingchengLanternFestaRules.NearestWithin(OverworldLayouts.PortoPolarLanterns, local, PortoPolarNightRules.MoonBoatwomanReach);
            if (lantern < 0 || lantern >= _lights.Length || !PortoPolarNightRules.IsOut(_lights[lantern]))
            {
                return;
            }

            Vector2 at = OverworldLayouts.PortoPolarLanterns[lantern];
            float distance = Vector2.Distance(at, local);
            bool boatwoman = Hero() == CharacterId.Lavine;
            if (distance > PortoPolarNightRules.ReachFor(boatwoman))
            {
                return;
            }

            bool fresh = Time.unscaledTime >= _nextPolarHint;
            if (_oilUsed[lantern])
            {
                PolarHint(fresh, LateCityEventsTexts.PolarNoOil);
                return;
            }

            int near = DarkNear(at);
            if (near > 0)
            {
                PolarHint(fresh, LateCityEventsTexts.PolarDarkNear);
                return;
            }

            if (!PortoPolarNightRules.CanRelight(_lights[lantern], _oilUsed[lantern], near, distance, boatwoman))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            PolarHint(fresh, LateCityEventsTexts.PolarRelight);
            if (!InteractPressed())
            {
                return;
            }

            _lights[lantern] = PortoPolarNightRules.RelightLight;
            _oilUsed[lantern] = true;
            _outSaid[lantern] = false;
            _nextPolarHint = Time.unscaledTime + HintCooldown;
            Say(LateCityEventsTexts.PolarRelit);
        }

        private int DarkNear(Vector2 lantern)
        {
            int near = 0;
            foreach (GameObject dark in _darks)
            {
                if (dark == null)
                {
                    continue;
                }

                Vector3 p = dark.transform.position;
                if (Vector2.Distance(new Vector2(p.x, p.z) - _portoOrigin, lantern) <= PortoPolarNightRules.RelightClearRadius)
                {
                    near++;
                }
            }

            return near;
        }

        private void PolarHint(bool fresh, string key)
        {
            if (!fresh)
            {
                return;
            }

            _nextPolarHint = Time.unscaledTime + HintCooldown;
            Say(key);
        }

        private void PayPolar(int day, int lit)
        {
            if (lit <= 0)
            {
                _polarOwedDay = int.MinValue;
                _polarPaidLocal = PortoPolarNightRules.PaidMark(day);
                SetMark(PortoPolarNightRules.PaidKey, _polarPaidLocal);
                Say(LateCityEventsTexts.PolarNone);
                return;
            }

            int gold = PortoPolarNightRules.GoldFor(lit);
            int stones = PortoPolarNightRules.StonesFor(lit);
            if (!Grant(gold, stones))
            {
                _polarOwedDay = day;
                _polarOwedLit = lit;
                _nextPolarOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _polarOwedDay = int.MinValue;
            _polarPaidLocal = PortoPolarNightRules.PaidMark(day);
            SetMark(PortoPolarNightRules.PaidKey, _polarPaidLocal);
            SayFormat(LateCityEventsTexts.PolarPaid, lit, gold, stones);
            if (lit >= PortoPolarNightRules.LanternCount)
            {
                CousinLine("polar");
            }
        }

        private bool EnsureDarkSpawning()
        {
            if (_view == null || !RegionBossDirector.SpawningEnabled)
            {
                return false;
            }

            if (_prefab == null)
            {
                EncounterArenaController encounter = FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
                _prefab = encounter != null ? encounter.EnemyPrefab : null;
            }

            if (_darkDefinition == null)
            {
                _darkDefinition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{PortoPolarNightRules.ShadowEnemyId}");
            }

            if (_lanternTargets == null)
            {
                Vector2[] lanterns = OverworldLayouts.PortoPolarLanterns;
                _lanternTargets = new Transform[lanterns.Length];
                for (int i = 0; i < lanterns.Length; i++)
                {
                    var target = new GameObject("Pl_PolarLantern_" + i);
                    target.transform.SetParent(transform, false);
                    _lanternTargets[i] = target.transform;
                }
            }

            for (int i = 0; i < _lanternTargets.Length; i++)
            {
                _lanternTargets[i].position = Ground(_portoOrigin + OverworldLayouts.PortoPolarLanterns[i]);
            }

            return _prefab != null && _darkDefinition != null;
        }

        /// <summary>Raises one wave at the three rises in turn; false (nothing raised, try again next frame) while a rise's chunk is still building.</summary>
        private bool SpawnDarkWave(int wave)
        {
            if (!EnsureDarkSpawning())
            {
                return false;
            }

            Vector2[] rises = OverworldLayouts.PortoPolarDarkRise;
            for (int i = 0; i < rises.Length; i++)
            {
                Vector2 world = _portoOrigin + rises[i];
                if (!Physics.Raycast(new Vector3(world.x, _view.GroundHeight(world) + GroundProbeHeight, world.y), Vector3.down, GroundProbeHeight * 2f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
            }

            int size = PortoPolarNightRules.WaveSize(wave);
            for (int i = 0; i < size; i++)
            {
                int rise = (wave + i) % rises.Length;
                float spread = (i / rises.Length - 0.5f) * 3f;
                SpawnDark(_portoOrigin + rises[rise] + new Vector2(spread, 0f), _lanternTargets[rise]);
            }

            return true;
        }

        private void SpawnDark(Vector2 world, Transform target)
        {
            float footHeight = EnemyVisualSwapper.GetFootHeight(_prefab.transform) * _prefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(world.x, _view.GroundHeight(world), world.y), footHeight);
            GameObject instance = Instantiate(_prefab, position, Quaternion.identity);
            instance.name = $"{_prefab.name}_{_darkDefinition.name}_noite_polar";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                // One waypoint: its lantern; a Dark walks there and drinks, unless the hero draws it off.
                controller.ResetForSpawn(_darkDefinition, position, new[] { target });
            }

            EnemyVisualSwapper.Apply(instance, _darkDefinition);
            EnemyLevelScaler.Attach(instance, PortoPolarNightRules.ShadowLevel);
            instance.AddComponent<MapMonsterInfo>().Configure(PortoPolarNightRules.ShadowLevel, OverworldLayouts.PortoLuaBrancaMapId, PolarAreaId, null);
            _darks.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
        }

        private void HandleEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy != null && _darks.Remove(evt.Enemy))
            {
                _corpses.Add(new KeyValuePair<GameObject, float>(evt.Enemy, Time.time + CorpseSeconds));
            }
        }

        private void ReleaseCorpses()
        {
            for (int i = _corpses.Count - 1; i >= 0; i--)
            {
                if (_corpses[i].Key == null || Time.time >= _corpses[i].Value)
                {
                    if (_corpses[i].Key != null)
                    {
                        Destroy(_corpses[i].Key);
                    }

                    _corpses.RemoveAt(i);
                }
            }
        }

        private void ClearDark()
        {
            StopPolar();
            foreach (KeyValuePair<GameObject, float> corpse in _corpses)
            {
                if (corpse.Key != null)
                {
                    Destroy(corpse.Key);
                }
            }

            _corpses.Clear();
        }

        private void ShowPolar()
        {
            Vector2[] lanterns = OverworldLayouts.PortoPolarLanterns;
            _lanternBeams ??= new LineRenderer[lanterns.Length];
            for (int i = 0; i < lanterns.Length && i < _lights.Length; i++)
            {
                float k = _lights[i] / PortoPolarNightRules.LightMax;
                Color color = PortoPolarNightRules.IsOut(_lights[i]) ? LanternOutColor : Color.Lerp(LanternLowColor, LanternLitColor, k);
                Beam(ref _lanternBeams[i], "Pl_PolarLantern", 0.3f + 0.4f * k, _portoOrigin + lanterns[i], LanternBeamHeight, color);
            }
        }

        private void HidePolar()
        {
            Hide(_lanternBeams);
        }
    }
}
