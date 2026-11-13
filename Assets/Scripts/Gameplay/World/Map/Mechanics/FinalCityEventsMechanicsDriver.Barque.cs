using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A Barca Atrasou of the Cais de Mesektet (71, <see cref="MesektetLateBarqueRules"/>): three ropes of shadow, the scarabs that guard them.</summary>
    public sealed partial class FinalCityEventsMechanicsDriver
    {
        private const float GroundProbeHeight = 6f;
        private const float CorpseSeconds = 5f;
        private const float RopePostHeight = 4f;
        private const float RopeLift = 1.6f;
        private const string BarqueAreaId = "cais_de_mesektet";

        private static readonly Color RopeTiedColor = new Color(0.16f, 0.08f, 0.26f, 0.95f);
        private static readonly Color RopeCutColor = new Color(0.95f, 0.82f, 0.45f, 0.9f);
        private static readonly Color BarqueSailColor = new Color(1f, 0.85f, 0.45f, 0.95f);

        private readonly bool[] _ropeCut = new bool[MesektetLateBarqueRules.RopeCount];
        private readonly List<GameObject> _scarabs = new List<GameObject>();
        private readonly List<KeyValuePair<GameObject, float>> _corpses = new List<KeyValuePair<GameObject, float>>();
        private GameObject _prefab;
        private EnemyDefinition _scarabDefinition;
        private Transform[] _postTargets;
        private LineRenderer[] _postBeams;
        private LineRenderer[] _ropeLines;
        private LineRenderer _barqueBeam;

        private bool _barqueRunning;
        private long _barqueRunIndex = long.MinValue;
        private long _barqueRopesIndex = long.MinValue;
        private long _barqueWarnedWindow = long.MinValue;
        private int _barqueWaves;
        private bool _barqueGuardsRaised;
        private long _barquePaidLocal;
        private long _barqueOwedIndex = -1L;
        private int _barqueOwedCut;
        private float _nextBarqueOwedTry;
        private float _nextBarqueHint;

        private void TickBarque(Vector2 local, DateTime utc)
        {
            ReleaseCorpses();
            float distance = Vector2.Distance(local, OverworldLayouts.CaisMesektetOffset);
            if (_barqueOwedIndex >= 0 && distance <= MesektetLateBarqueRules.LeaveRange && Time.unscaledTime >= _nextBarqueOwedTry)
            {
                PayBarque(_barqueOwedIndex, _barqueOwedCut);
            }

            RealTimeWindow window = MesektetLateBarqueRules.WindowAt(utc);
            if (!_barqueRunning)
            {
                HideBarque();
                if (distance > MesektetLateBarqueRules.AlertRange)
                {
                    return;
                }

                if (!window.IsOpen)
                {
                    if (MesektetLateBarqueRules.IsWarning(window) && _barqueWarnedWindow != window.Index)
                    {
                        _barqueWarnedWindow = window.Index;
                        SayFormat(FinalCityEventsTexts.BarqueWarning, WorldEventCalendar.FormatCountdown(window.SecondsToChange));
                    }

                    return;
                }

                long paid = GetMark(MesektetLateBarqueRules.PaidKey, _barquePaidLocal);
                if (MesektetLateBarqueRules.CanStart(true, paid, window.Index, distance) && _barqueOwedIndex != window.Index)
                {
                    StartBarque(window.Index, MesektetLateBarqueRules.SecondsInto(window, utc));
                }

                return;
            }

            // A defence under way ends when the Mesektet sails, when the window closes or when abandoned.
            if (distance > MesektetLateBarqueRules.LeaveRange)
            {
                Say(FinalCityEventsTexts.BarqueAbandoned);
                StopBarque();
                return;
            }

            if (!window.IsOpen || window.Index != _barqueRunIndex)
            {
                long index = _barqueRunIndex;
                int cut = MesektetLateBarqueRules.CutCount(_ropeCut);
                StopBarque();
                Say(FinalCityEventsTexts.BarqueDragged);
                PayBarque(index, cut);
                return;
            }

            double t = MesektetLateBarqueRules.SecondsInto(window, utc);
            _scarabs.RemoveAll(scarab => scarab == null);
            int tied = MesektetLateBarqueRules.RopeCount - MesektetLateBarqueRules.CutCount(_ropeCut);
            if (!_barqueGuardsRaised)
            {
                if (SpawnGuards(MesektetLateBarqueRules.StartGuards(tied)))
                {
                    _barqueGuardsRaised = true;
                }
            }
            else if (_barqueWaves < MesektetLateBarqueRules.ReinforcementsDue(t))
            {
                int size = MesektetLateBarqueRules.WaveSize(tied, _scarabs.Count);
                if (size <= 0 || SpawnGuards(size))
                {
                    _barqueWaves++;
                    if (size > 0)
                    {
                        Say(FinalCityEventsTexts.BarqueMoreScarabs);
                    }
                }
            }

            if (_stats.IsAlive)
            {
                TickCut(local);
            }

            if (MesektetLateBarqueRules.Sails(_ropeCut))
            {
                long index = _barqueRunIndex;
                StopBarque();
                Say(FinalCityEventsTexts.BarqueSails);
                PayBarque(index, MesektetLateBarqueRules.RopeCount);
                return;
            }

            ShowBarque();
        }

        private void StartBarque(long index, double secondsIn)
        {
            if (!EnsureScarabSpawning())
            {
                return;
            }

            if (_barqueRopesIndex != index)
            {
                // A new window: every rope tied again. Within the same window the ropes keep what was cut (leaving and coming back resets nothing).
                _barqueRopesIndex = index;
                for (int i = 0; i < _ropeCut.Length; i++)
                {
                    _ropeCut[i] = false;
                }
            }

            _barqueRunning = true;
            _barqueRunIndex = index;
            _barqueGuardsRaised = false;
            _barqueWaves = MesektetLateBarqueRules.ReinforcementsDue(secondsIn);
            Say(FinalCityEventsTexts.BarqueStart);
        }

        /// <summary>Ends the defence: the living scarabs go back into the river (the dead keep their few seconds of corpse).</summary>
        private void StopBarque()
        {
            foreach (GameObject scarab in _scarabs)
            {
                if (scarab != null)
                {
                    Destroy(scarab);
                }
            }

            _scarabs.Clear();
            _barqueRunning = false;
            HideBarque();
        }

        private void TickCut(Vector2 local)
        {
            Vector2[] posts = OverworldLayouts.MesektetShadowPosts;
            int post = MingchengLanternFestaRules.NearestWithin(posts, local, MesektetLateBarqueRules.LightReach);
            if (post < 0 || post >= _ropeCut.Length || _ropeCut[post])
            {
                return;
            }

            float distance = Vector2.Distance(posts[post], local);
            bool light = Hero() == CharacterId.Maria;
            if (distance > MesektetLateBarqueRules.ReachFor(light))
            {
                return;
            }

            bool fresh = Time.unscaledTime >= _nextBarqueHint;
            int near = ScarabsNear(posts[post]);
            if (near > 0)
            {
                BarqueHint(fresh, FinalCityEventsTexts.BarqueGuarded);
                return;
            }

            if (!MesektetLateBarqueRules.CanCut(true, near, distance, light))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            BarqueHint(fresh, FinalCityEventsTexts.BarqueCut);
            if (!InteractPressed())
            {
                return;
            }

            _ropeCut[post] = true;
            _nextBarqueHint = Time.unscaledTime + HintCooldown;
            SayFormat(FinalCityEventsTexts.BarqueRopeCut, SideName(post), MesektetLateBarqueRules.CutCount(_ropeCut), MesektetLateBarqueRules.RopeCount);
        }

        private int ScarabsNear(Vector2 post)
        {
            int near = 0;
            foreach (GameObject scarab in _scarabs)
            {
                if (scarab == null)
                {
                    continue;
                }

                Vector3 p = scarab.transform.position;
                if (Vector2.Distance(new Vector2(p.x, p.z) - _mesektetOrigin, post) <= MesektetLateBarqueRules.GuardClearRadius)
                {
                    near++;
                }
            }

            return near;
        }

        private void BarqueHint(bool fresh, string key)
        {
            if (!fresh)
            {
                return;
            }

            _nextBarqueHint = Time.unscaledTime + HintCooldown;
            Say(key);
        }

        private void PayBarque(long index, int cut)
        {
            if (cut <= 0)
            {
                _barqueOwedIndex = -1L;
                _barquePaidLocal = MesektetLateBarqueRules.PaidMark(index);
                SetMark(MesektetLateBarqueRules.PaidKey, _barquePaidLocal);
                Say(FinalCityEventsTexts.BarqueNone);
                return;
            }

            int gold = MesektetLateBarqueRules.GoldFor(cut);
            int stones = MesektetLateBarqueRules.StonesFor(cut);
            if (!Grant(gold, stones))
            {
                _barqueOwedIndex = index;
                _barqueOwedCut = cut;
                _nextBarqueOwedTry = Time.unscaledTime + 5f;
                return;
            }

            _barqueOwedIndex = -1L;
            _barquePaidLocal = MesektetLateBarqueRules.PaidMark(index);
            SetMark(MesektetLateBarqueRules.PaidKey, _barquePaidLocal);
            SayFormat(FinalCityEventsTexts.BarquePaid, cut, gold, stones);
            if (cut >= MesektetLateBarqueRules.RopeCount)
            {
                CousinLine("barque");
            }
        }

        private bool EnsureScarabSpawning()
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

            if (_scarabDefinition == null)
            {
                _scarabDefinition = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{MesektetLateBarqueRules.GuardEnemyId}");
            }

            if (_postTargets == null)
            {
                Vector2[] posts = OverworldLayouts.MesektetShadowPosts;
                _postTargets = new Transform[posts.Length];
                for (int i = 0; i < posts.Length; i++)
                {
                    var target = new GameObject("Ms_ShadowPost_" + i);
                    target.transform.SetParent(transform, false);
                    _postTargets[i] = target.transform;
                }
            }

            for (int i = 0; i < _postTargets.Length; i++)
            {
                _postTargets[i].position = Ground(_mesektetOrigin + OverworldLayouts.MesektetShadowPosts[i]);
            }

            return _prefab != null && _scarabDefinition != null;
        }

        /// <summary>
        /// Raises <paramref name="count"/> scarabs at the rises of the posts still tied, in turn; false (nothing raised, try again next frame) while
        /// a rise's chunk is still building.
        /// </summary>
        private bool SpawnGuards(int count)
        {
            if (count <= 0)
            {
                return true;
            }

            if (!EnsureScarabSpawning())
            {
                return false;
            }

            Vector2[] rises = OverworldLayouts.MesektetScarabRise;
            var tied = new List<int>();
            for (int i = 0; i < rises.Length && i < _ropeCut.Length; i++)
            {
                if (_ropeCut[i])
                {
                    continue;
                }

                Vector2 world = _mesektetOrigin + rises[i];
                if (!Physics.Raycast(new Vector3(world.x, _view.GroundHeight(world) + GroundProbeHeight, world.y), Vector3.down, GroundProbeHeight * 2f,
                        Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }

                tied.Add(i);
            }

            if (tied.Count == 0)
            {
                return true;
            }

            for (int i = 0; i < count; i++)
            {
                int rise = tied[i % tied.Count];
                float spread = (i / tied.Count - 0.5f) * 3f;
                SpawnScarab(_mesektetOrigin + rises[rise] + new Vector2(spread, 0f), _postTargets[rise]);
            }

            return true;
        }

        private void SpawnScarab(Vector2 world, Transform target)
        {
            float footHeight = EnemyVisualSwapper.GetFootHeight(_prefab.transform) * _prefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(world.x, _view.GroundHeight(world), world.y), footHeight);
            GameObject instance = Instantiate(_prefab, position, Quaternion.identity);
            instance.name = $"{_prefab.name}_{_scarabDefinition.name}_barca_atrasou";

            EnemyController controller = instance.GetComponent<EnemyController>();
            if (controller != null)
            {
                // One waypoint: its post; a scarab walks there and guards the rope, unless the hero draws it off.
                controller.ResetForSpawn(_scarabDefinition, position, new[] { target });
            }

            EnemyVisualSwapper.Apply(instance, _scarabDefinition);
            EnemyLevelScaler.Attach(instance, MesektetLateBarqueRules.GuardLevel);
            instance.AddComponent<MapMonsterInfo>().Configure(MesektetLateBarqueRules.GuardLevel, OverworldLayouts.CaisMesektetMapId, BarqueAreaId, null);
            _scarabs.Add(instance);
            EventBus.Publish(new EnemySpawnedEvent(instance));
        }

        private void HandleEnemyDied(EnemyDiedEvent evt)
        {
            if (evt.Enemy != null && _scarabs.Remove(evt.Enemy))
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

        private void ClearScarabs()
        {
            StopBarque();
            foreach (KeyValuePair<GameObject, float> corpse in _corpses)
            {
                if (corpse.Key != null)
                {
                    Destroy(corpse.Key);
                }
            }

            _corpses.Clear();
        }

        private void ShowBarque()
        {
            Vector2[] posts = OverworldLayouts.MesektetShadowPosts;
            _postBeams ??= new LineRenderer[posts.Length];
            _ropeLines ??= new LineRenderer[posts.Length];
            Vector3 barque = Ground(_mesektetOrigin + OverworldLayouts.MesektetBarqueBerth) + Vector3.up * RopeLift;
            for (int i = 0; i < posts.Length && i < _ropeCut.Length; i++)
            {
                Color color = _ropeCut[i] ? RopeCutColor : RopeTiedColor;
                Vector2 post = _mesektetOrigin + posts[i];
                Beam(ref _postBeams[i], "Ms_ShadowPost", _ropeCut[i] ? 0.3f : 0.55f, post, RopePostHeight, color);
                if (_ropeCut[i])
                {
                    Hide(_ropeLines[i]);
                }
                else
                {
                    // The rope of shadow from the post to the Mesektet's hull.
                    Post(ref _ropeLines[i], "Ms_ShadowRope", 0.25f, Ground(post) + Vector3.up * RopeLift, barque, RopeTiedColor);
                }
            }

            int cut = MesektetLateBarqueRules.CutCount(_ropeCut);
            Beam(ref _barqueBeam, "Ms_Mesektet", 0.4f + 0.2f * cut, _mesektetOrigin + OverworldLayouts.MesektetBarqueBerth, 3f + 2f * cut,
                Color.Lerp(RopeTiedColor, BarqueSailColor, cut / (float)MesektetLateBarqueRules.RopeCount));
        }

        private void HideBarque()
        {
            Hide(_postBeams);
            Hide(_ropeLines);
            Hide(_barqueBeam);
        }
    }
}
