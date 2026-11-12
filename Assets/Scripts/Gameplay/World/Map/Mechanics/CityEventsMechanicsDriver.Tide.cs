using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The Maré de Lodo of Jhilpur (31, <see cref="JhilpurMudTideRules"/>): waves of Filhos do Lodo from the lake to the foot of the ghats.</summary>
    public sealed partial class CityEventsMechanicsDriver
    {
        private const float GroundProbeHeight = 6f;
        private const float CorpseSeconds = 5f;
        private const float SiltSayEvery = 4f;
        private const float ShoreBeamHeight = 7f;
        private const string TideAreaId = "lago_de_jhilpur";

        private static readonly Color ShoreColor = new Color(0.55f, 0.42f, 0.22f, 0.9f);
        private static readonly Color BandCleanColor = new Color(0.3f, 0.85f, 0.75f, 0.85f);
        private static readonly Color BandSiltColor = new Color(0.45f, 0.3f, 0.12f, 0.95f);

        private readonly List<GameObject> _filhos = new List<GameObject>();
        private readonly List<KeyValuePair<GameObject, float>> _corpses = new List<KeyValuePair<GameObject, float>>();
        private GameObject _prefab;
        private EnemyDefinition _filhoDefinition;
        private Transform[] _ghatTargets;
        private LineRenderer[] _shoreBeams;
        private LineRenderer _ghatBand;

        private bool _tideRunning;
        private long _tideIndex;
        private int _wavesSpawned;
        private float _sinceWave;
        private float _silt;
        private float _tideRestUntil;
        private float _nextSiltSay;
        private long _tideWarned = long.MinValue;
        private long _tidePaidLocal;
        private long _tideOwedIndex = -1L;
        private float _tideOwedSilt;
        private float _nextTideOwedTry;

        private void TickTide(Vector2 local, DateTime utc)
        {
            ReleaseCorpses();
            float distance = Vector2.Distance(local, OverworldLayouts.JhilpurOffset);
            RealTimeWindow window = JhilpurMudTideRules.WindowAt(utc);
            if (_tideOwedIndex >= 0 && distance <= JhilpurMudTideRules.LeaveRange && Time.unscaledTime >= _nextTideOwedTry)
            {
                PayTide(_tideOwedIndex, _tideOwedSilt);
            }

            if (!_tideRunning)
            {
                HideTide();
                if (distance > JhilpurMudTideRules.AlertRange)
                {
                    return;
                }

                if (JhilpurMudTideRules.IsWarning(window) && _tideWarned != window.Index)
                {
                    _tideWarned = window.Index;
                    SayFormat(CityEventsTexts.TideWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                }

                long paid = GetMark(JhilpurMudTideRules.PaidKey, _tidePaidLocal);
                if (JhilpurMudTideRules.CanStart(window, paid, distance, Time.time < _tideRestUntil) && _tideOwedIndex != window.Index)
                {
                    StartTide(window.Index);
                }

                return;
            }

            // A defence under way is not cut when the window closes (as the world events); it ends when won, silted or abandoned.
            if (distance > JhilpurMudTideRules.LeaveRange)
            {
                Say(CityEventsTexts.TideAbandoned);
                StopTide(false);
                return;
            }

            _filhos.RemoveAll(filho => filho == null);
            int alive = _filhos.Count;
            _sinceWave += Time.deltaTime;
            if (JhilpurMudTideRules.ShouldSpawnWave(_wavesSpawned, alive, _sinceWave) && SpawnWave(_wavesSpawned))
            {
                _wavesSpawned++;
                _sinceWave = 0f;
                SayFormat(CityEventsTexts.TideWave, _wavesSpawned, JhilpurMudTideRules.WaveCount);
                alive = _filhos.Count;
            }

            int silters = 0;
            foreach (GameObject filho in _filhos)
            {
                Vector3 p = filho.transform.position;
                if (JhilpurMudTideRules.Silting(OverworldLayouts.JhilpurGhatBand, new Vector2(p.x, p.z) - _deltaOrigin))
                {
                    silters++;
                }
            }

            _silt = JhilpurMudTideRules.AdvanceSilt(_silt, silters, Time.deltaTime);
            if (silters > 0 && Time.unscaledTime >= _nextSiltSay)
            {
                _nextSiltSay = Time.unscaledTime + SiltSayEvery;
                SayFormat(CityEventsTexts.TideSilt, JhilpurMudTideRules.Percent(_silt));
            }

            ShowTide();
            switch (JhilpurMudTideRules.Resolve(_wavesSpawned, alive, _silt))
            {
                case MudTideOutcome.Won:
                    long index = _tideIndex;
                    float silt = _silt;
                    StopTide(true);
                    Say(CityEventsTexts.TideWon);
                    CousinLine("tide");
                    PayTide(index, silt);
                    break;
                case MudTideOutcome.Silted:
                    Say(CityEventsTexts.TideSilted);
                    StopTide(false);
                    _tideRestUntil = Time.time + JhilpurMudTideRules.RestSeconds;
                    break;
            }
        }

        private void StartTide(long windowIndex)
        {
            if (!EnsureTideSpawning())
            {
                return;
            }

            _tideRunning = true;
            _tideIndex = windowIndex;
            _wavesSpawned = 0;
            _sinceWave = JhilpurMudTideRules.WaveIntervalSeconds;
            _silt = 0f;
            _nextSiltSay = 0f;
            Say(CityEventsTexts.TideStart);
        }

        /// <summary>Ends the defence: the living Filhos sink back into the lake (the dead keep their few seconds of corpse).</summary>
        private void StopTide(bool won)
        {
            foreach (GameObject filho in _filhos)
            {
                if (filho != null)
                {
                    Destroy(filho);
                }
            }

            _filhos.Clear();
            _tideRunning = false;
            _wavesSpawned = 0;
            if (!won)
            {
                _silt = 0f;
            }

            HideTide();
        }

        private void PayTide(long index, float silt)
        {
            int stones = JhilpurMudTideRules.StonesFor(silt);
            if (!Grant(JhilpurMudTideRules.RewardGold, stones))
            {
                _tideOwedIndex = index;
                _tideOwedSilt = silt;
                _nextTideOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _tideOwedIndex = -1L;
            _tidePaidLocal = JhilpurMudTideRules.PaidMark(index);
            SetMark(JhilpurMudTideRules.PaidKey, _tidePaidLocal);
            SayFormat(CityEventsTexts.TidePaid, JhilpurMudTideRules.RewardGold, stones);
        }

        private bool EnsureTideSpawning()
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

            if (_filhoDefinition == null)
            {
                _filhoDefinition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{JhilpurMudTideRules.FilhoEnemyId}");
            }

            if (_ghatTargets == null)
            {
                Vector2[] feet = OverworldLayouts.JhilpurGhatFoot;
                _ghatTargets = new Transform[feet.Length];
                for (int i = 0; i < feet.Length; i++)
                {
                    var target = new GameObject("Jh_GhatFoot_" + i);
                    target.transform.SetParent(transform, false);
                    _ghatTargets[i] = target.transform;
                }
            }

            for (int i = 0; i < _ghatTargets.Length; i++)
            {
                _ghatTargets[i].position = Ground(_deltaOrigin + OverworldLayouts.JhilpurGhatFoot[i]);
            }

            return _prefab != null && _filhoDefinition != null;
        }

        /// <summary>Raises one wave on the three shores in turn; false (nothing raised, try again next frame) while the shore's chunk is still building.</summary>
        private bool SpawnWave(int wave)
        {
            if (!EnsureTideSpawning())
            {
                return false;
            }

            Vector2[] shores = OverworldLayouts.JhilpurTideShore;
            int size = JhilpurMudTideRules.WaveSize(wave);
            for (int i = 0; i < shores.Length; i++)
            {
                Vector2 world = _deltaOrigin + shores[i];
                if (!Physics.Raycast(new Vector3(world.x, _view.GroundHeight(world) + GroundProbeHeight, world.y), Vector3.down, GroundProbeHeight * 2f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }
            }

            for (int i = 0; i < size; i++)
            {
                int shore = (wave + i) % shores.Length;
                float spread = (i / shores.Length - 0.5f) * 3f;
                SpawnFilho(_deltaOrigin + shores[shore] + new Vector2(spread, 0f), _ghatTargets[shore]);
            }

            return true;
        }

        private void SpawnFilho(Vector2 world, Transform target)
        {
            float footHeight = EnemyVisualSwapper.GetFootHeight(_prefab.transform) * _prefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(world.x, _view.GroundHeight(world), world.y), footHeight);
            GameObject instance = Instantiate(_prefab, position, Quaternion.identity);
            instance.name = $"{_prefab.name}_{_filhoDefinition.name}_mare_de_lodo";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                // One waypoint: the foot of the ghats; a Filho walks there and stays, unless the hero draws it off.
                controller.ResetForSpawn(_filhoDefinition, position, new[] { target });
            }

            EnemyVisualSwapper.Apply(instance, _filhoDefinition);
            EnemyLevelScaler.Attach(instance, JhilpurMudTideRules.FilhoLevel);
            instance.AddComponent<MapMonsterInfo>().Configure(JhilpurMudTideRules.FilhoLevel, OverworldLayouts.JhilpurMapId, TideAreaId, null);
            _filhos.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
        }

        private void HandleEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy != null && _filhos.Remove(evt.Enemy))
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

        private void ClearFilhos()
        {
            StopTide(false);
            foreach (KeyValuePair<GameObject, float> corpse in _corpses)
            {
                if (corpse.Key != null)
                {
                    Destroy(corpse.Key);
                }
            }

            _corpses.Clear();
        }

        private void ShowTide()
        {
            Vector2[] shores = OverworldLayouts.JhilpurTideShore;
            _shoreBeams ??= new LineRenderer[shores.Length];
            for (int i = 0; i < shores.Length; i++)
            {
                Beam(ref _shoreBeams[i], "Jh_TideShore", 1.2f, _deltaOrigin + shores[i], ShoreBeamHeight, ShoreColor);
            }

            Rect band = OverworldLayouts.JhilpurGhatBand;
            Vector2 a = _deltaOrigin + new Vector2(band.xMin, band.center.y);
            Vector2 b = _deltaOrigin + new Vector2(band.xMax, band.center.y);
            Color color = Color.Lerp(BandCleanColor, BandSiltColor, _silt / JhilpurMudTideRules.SiltMax);
            Post(ref _ghatBand, "Jh_GhatBand", 1f, Ground(a) + Vector3.up * 0.2f, Ground(b) + Vector3.up * 0.2f, color);
        }

        private void HideTide()
        {
            Hide(_shoreBeams);
            Hide(_ghatBand);
        }
    }
}
