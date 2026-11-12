using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the Estepe dos Nartes mechanics (the rules are <see cref="NartesRaceSession"/>, <see cref="NartesChainRules"/>
    /// and <see cref="NartesSkyGoldRules"/>): added to the overworld root, idle unless the layout holds the region and the hero
    /// stands in it. The hero keeps the controls all the time. The UTC-day claims are saved per slot as daily marks of
    /// <see cref="WorldMapProgress"/>; the rules take the day as a number, so a server can hold it.
    /// </summary>
    public sealed partial class NartesMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string GoldItemId = "item_gold";
        private const string DayPrefix = "nartes.day.";
        public const string RaceDayKey = "race";
        public const string ChainDayKey = "chain_top";
        public const string GoldDayKeyPrefix = "gold";
        private const float HintCooldown = 3f;
        private const float FeedbackHeight = 2.4f;
        private const float ChainViewRange = 130f;
        private const float GoldViewRange = 60f;
        private const float StartViewRange = 200f;
        private const float GateBeaconHeight = 26f;
        private const int RingSegments = 32;

        private static readonly Color GoldColor = new Color(1f, 0.82f, 0.3f, 1f);
        private static readonly Color IronColor = new Color(0.45f, 0.5f, 0.56f, 0.9f);
        private static readonly Color LitColor = new Color(0.95f, 0.9f, 0.6f, 1f);
        private static readonly Color WarnColor = new Color(1f, 0.55f, 0.2f, 1f);
        private static readonly Color GustColor = new Color(0.85f, 0.95f, 1f, 1f);

        private readonly List<LineRenderer> _anchorBeacons = new List<LineRenderer>();
        private readonly List<LineRenderer> _goldBeacons = new List<LineRenderer>();
        private readonly HeroRequirementCache _heroState = new HeroRequirementCache();
        private string[] _goldKeys;

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Vector2[] _anchors;
        private float _chainLength;
        private Transform _player;
        private PlayerController _controller;
        private CharacterController _character;
        private InventoryManager _bag;
        private PlayerStats _stats;
        private Material _material;

        private NartesRaceSession _race;
        private int _spokenCount = -1;
        private LineRenderer _startBeacon;
        private LineRenderer _gateBeacon;
        private LineRenderer _gateRing;
        private int _gateDrawn = -1;

        private NartesChainClimb _climb;
        private float _blownUntil = float.NegativeInfinity;
        private bool _saidWarn;
        private bool _saidGust;
        private bool _saidTop;
        private LineRenderer _gustLine;

        private float _nextHint;
        private int _hintGold = -1;
        private bool _hintRace;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _race != null || _climb != null;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.NartesAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.NartesBounds, _origin) };
            _anchors = NartesChainRules.Anchors(OverworldLayouts.AmiraniChain);
            _chainLength = NartesChainRules.Length(OverworldLayouts.AmiraniChain);
        }

        private void OnEnable() => _heroState.Attach();

        private void OnDisable()
        {
            HideFeedback();
            LeaveSteppe();
            _heroState.Detach();
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Update()
        {
            if (!FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            bool alive = _stats == null || _stats.IsAlive;
            if (!OverworldLayouts.NartesBounds.Contains(local) || !alive)
            {
                AbandonRace();
                EndClimb();
                HideFeedback();
                LeaveSteppe();
                return;
            }

            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            long today = NartesRaceRules.DayOf((long)Math.Floor(now));
            TickRace(local, today);
            TickChain(local, now, today);
            TickGolds(local, today);

            // 49, 50, 51 (NartesMechanicsDriver.Steppe.cs).
            TickSteppe(local, now, today);
        }

        private bool FindPlayer()
        {
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return false;
                }

                _player = go.transform;
                _controller = go.GetComponent<PlayerController>();
                _character = go.GetComponent<CharacterController>();
                _bag = go.GetComponentInChildren<InventoryManager>();
                _stats = go.GetComponent<PlayerStats>();
            }

            return _controller != null && _character != null;
        }

        private bool ZoneOpen(Vector2 local)
        {
            MapRequirementState state = _heroState.Get(_bag);
            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            OverworldZone zone = OverworldRules.ZoneAt(_layout, _origin + local);
            return zone != null && OverworldRules.CheckEnter(zone, level, state.Hero, _heroState.Lookup) == MapAccessCheck.Ok;
        }

        private static bool Mounted => MountController.Active != null && MountController.Active.IsMounted;

        /// <summary>The UTC day this claim was last taken in this save, or <see cref="NartesSkyGoldRules.NeverTaken"/>.</summary>
        public static long ClaimedDay(WorldMapProgress progress, string key)
        {
            int day = progress != null ? progress.GetDailyMark(DayPrefix + key) : 0;
            return day > 0 ? day : NartesSkyGoldRules.NeverTaken;
        }

        public static void StoreClaim(WorldMapProgress progress, string key, long day) => progress?.SetDailyMark(DayPrefix + key, (int)day);

        private static long LastDay(string key) => ClaimedDay(ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null, key);

        private static void SetDay(string key, long day) => StoreClaim(ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null, key, day);

        /// <summary>Puts nuggets in the backpack; false (and nothing owed) when they do not all fit.</summary>
        private bool Pay(int amount)
        {
            ItemDefinition gold = ItemDatabase.GetById(GoldItemId);
            if (gold == null || _bag == null || _bag.AddItem(gold, amount) > 0)
            {
                Say(NartesMechanicsTexts.BagFull);
                return false;
            }

            return true;
        }

        // ------------------------------------------------------------------
        // The race
        // ------------------------------------------------------------------

        private void TickRace(Vector2 local, long today)
        {
            if (_race == null)
            {
                float toStart = Vector2.Distance(local, OverworldLayouts.NartesRaceStart);
                ShowStart(toStart < StartViewRange);
                if (toStart > NartesRaceRules.StartReach)
                {
                    _hintRace = false;
                    return;
                }

                if (!_hintRace || Time.unscaledTime >= _nextHint)
                {
                    _hintRace = true;
                    _nextHint = Time.unscaledTime + HintCooldown;
                    Say(NartesMechanicsTexts.RaceCall);
                }

                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (_controller.Input.InteractPressedThisFrame && ZoneOpen(OverworldLayouts.NartesRaceStart))
                {
                    _race = new NartesRaceSession(OverworldLayouts.NartesRaceCourse);
                    _spokenCount = -1;
                    _gateDrawn = -1;
                    ShowStart(false);
                }

                return;
            }

            NartesRaceState before = _race.State;
            bool took = _race.Tick(Time.deltaTime, local);
            if (_race.State == NartesRaceState.Countdown && _race.CountdownLeft != _spokenCount)
            {
                _spokenCount = _race.CountdownLeft;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.RaceCount, _spokenCount));
            }

            if (before == NartesRaceState.Countdown && _race.State == NartesRaceState.Running)
            {
                Say(NartesMechanicsTexts.RaceGo);
            }

            if (_race.IsOver)
            {
                FinishRace(today);
                return;
            }

            if (took)
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.RaceGate, _race.NextGate, _race.GateCount, Mathf.FloorToInt(_race.Elapsed)));
            }

            ShowGate();
        }

        private void FinishRace(long today)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            switch (_race.State)
            {
                case NartesRaceState.Finished:
                    NartesRaceMedal medal = NartesRaceRules.MedalFor(_race.Elapsed);
                    if (medal == NartesRaceMedal.Gold)
                    {
                        EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesRaceGold));
                    }

                    string seconds = _race.Elapsed.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
                    if (medal == NartesRaceMedal.None)
                    {
                        SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.RaceNoMedal, seconds));
                        break;
                    }

                    bool first = LastDay(RaceDayKey) != today;
                    int purse = NartesRaceRules.Purse(medal, first);
                    if (Pay(purse))
                    {
                        if (first)
                        {
                            SetDay(RaceDayKey, today);
                        }

                        SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.RaceMedal,
                            NartesMechanicsTexts.Get(localization, NartesMechanicsTexts.Medals[(int)medal]), seconds, purse));
                    }

                    break;
                case NartesRaceState.TimedOut:
                    Say(NartesMechanicsTexts.RaceTimeOut);
                    break;
                default:
                    Say(NartesMechanicsTexts.RaceAbandon);
                    break;
            }

            _race = null;
            HideGate();
        }

        private void AbandonRace()
        {
            if (_race != null)
            {
                _race.Abandon();
                Say(NartesMechanicsTexts.RaceAbandon);
                _race = null;
                HideGate();
            }
        }

        private void ShowStart(bool show)
        {
            if (show && _startBeacon == null)
            {
                _startBeacon = NewLine("Nartes_RaceStart", 0.5f);
                Vector3 foot = Ground(OverworldLayouts.NartesRaceStart);
                _startBeacon.startColor = _startBeacon.endColor = GoldColor;
                _startBeacon.SetPosition(0, foot);
                _startBeacon.SetPosition(1, foot + Vector3.up * GateBeaconHeight);
            }

            if (_startBeacon != null)
            {
                _startBeacon.enabled = show;
            }
        }

        private void ShowGate()
        {
            _gateBeacon = _gateBeacon != null ? _gateBeacon : NewLine("Nartes_RaceGate", 0.7f);
            _gateRing = _gateRing != null ? _gateRing : NewLine("Nartes_RaceRing", 0.35f);
            _gateBeacon.enabled = _gateRing.enabled = true;
            if (_gateDrawn == _race.NextGate)
            {
                return;
            }

            _gateDrawn = _race.NextGate;
            Vector2 gate = _race.NextGatePosition;
            Vector3 foot = Ground(gate);
            _gateBeacon.startColor = _gateBeacon.endColor = GoldColor;
            _gateBeacon.SetPosition(0, foot);
            _gateBeacon.SetPosition(1, foot + Vector3.up * GateBeaconHeight);
            _gateRing.startColor = _gateRing.endColor = GoldColor;
            _gateRing.positionCount = RingSegments + 1;
            for (int i = 0; i <= RingSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / RingSegments;
                _gateRing.SetPosition(i, Ground(gate + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * NartesRaceRules.GateRadius) + Vector3.up * 0.4f);
            }
        }

        private void HideGate()
        {
            if (_gateBeacon != null)
            {
                _gateBeacon.enabled = false;
            }

            if (_gateRing != null)
            {
                _gateRing.enabled = false;
            }
        }

        // ------------------------------------------------------------------
        // Amirani's chain
        // ------------------------------------------------------------------

        private void TickChain(Vector2 local, double now, long today)
        {
            Vector2[] line = OverworldLayouts.AmiraniChain;
            NartesChainRules.Project(line, local, out float along, out float distance, out Vector2 across);
            ShowAnchors(local, distance);
            bool onLinks = distance <= NartesChainRules.HalfWidth;
            if (_climb == null)
            {
                ShowGust(WindGustPhase.Calm, Vector2.zero);
                if (!onLinks)
                {
                    return;
                }

                if (Mounted)
                {
                    if (Time.unscaledTime >= _nextHint)
                    {
                        _nextHint = Time.unscaledTime + HintCooldown;
                        Say(NartesMechanicsTexts.ChainMounted);
                    }

                    return;
                }

                if (ZoneOpen(local))
                {
                    _climb = new NartesChainClimb(_anchors.Length, _chainLength, along);
                    _saidTop = false;
                    Say(NartesMechanicsTexts.ChainEnter);
                }

                return;
            }

            if (Mounted)
            {
                EndClimb();
                return;
            }

            if (!onLinks)
            {
                if (Time.time < _blownUntil)
                {
                    Fall();
                }
                else
                {
                    EndClimb();
                }

                return;
            }

            _climb.Reach(along);
            if (_climb.ReachedTop && !_saidTop)
            {
                _saidTop = true;
                ReachTop(today);
            }

            WindGustPhase phase = NartesChainRules.PhaseAt(now);
            bool sheltered = NartesChainRules.IsSheltered(_anchors, local);
            Vector2 push = across * NartesChainRules.GustSide(now) * NartesChainRules.PushSpeed;
            ShowGust(phase, push);
            SayGust(phase, sheltered);
            if (phase != WindGustPhase.Blowing || sheltered || !_character.enabled)
            {
                return;
            }

            _blownUntil = Time.time + NartesChainRules.BlownGraceSeconds;
            GroundedPush.Shove(_character, push * Time.deltaTime);
        }

        private void SayGust(WindGustPhase phase, bool sheltered)
        {
            if (phase == WindGustPhase.Calm)
            {
                _saidWarn = _saidGust = false;
            }
            else if (phase == WindGustPhase.Warning && !_saidWarn)
            {
                _saidWarn = true;
                Say(NartesMechanicsTexts.ChainWarn);
            }
            else if (phase == WindGustPhase.Blowing && !sheltered && !_saidGust)
            {
                _saidGust = true;
                Say(NartesMechanicsTexts.ChainGust);
            }
        }

        private void Fall()
        {
            if (_stats != null)
            {
                float damage = NartesChainRules.FallDamage(_stats.CurrentHealth, _stats.MaxHealth);
                if (damage > 0f)
                {
                    _stats.ApplyHazardDamage(damage);
                }
            }

            _blownUntil = float.NegativeInfinity;
            Say(NartesMechanicsTexts.ChainFall);
            int ring = Mathf.Clamp(_climb.SafeAnchor, 0, _anchors.Length - 1);
            Vector2 end = _origin + _anchors[ring];
            Vector2 heading = _anchors[Mathf.Min(ring + 1, _anchors.Length - 1)] - _anchors[ring];
            _controller.ClickToMove?.ClearDestination();
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, heading.sqrMagnitude > 0.01f ? heading : Vector2.up);
            _view.ResetTracking(end);
        }

        private void ReachTop(long today)
        {
            EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesChainTop));
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (LastDay(ChainDayKey) == today)
            {
                Say(NartesMechanicsTexts.ChainTopDone);
            }
            else if (Pay(NartesChainRules.TopPurse))
            {
                SetDay(ChainDayKey, today);
                SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.ChainTop, NartesChainRules.TopPurse));
            }
        }

        private void EndClimb()
        {
            _climb = null;
            _blownUntil = float.NegativeInfinity;
            _saidWarn = _saidGust = false;
            ShowGust(WindGustPhase.Calm, Vector2.zero);
        }

        private void ShowAnchors(Vector2 local, float distance)
        {
            if (distance >= ChainViewRange)
            {
                HideLines(_anchorBeacons);
                return;
            }

            if (_anchorBeacons.Count == 0)
            {
                foreach (Vector2 anchor in _anchors)
                {
                    LineRenderer beacon = NewLine("Nartes_ChainRing", 0.3f);
                    Vector3 foot = Ground(anchor);
                    beacon.SetPosition(0, foot);
                    beacon.SetPosition(1, foot + Vector3.up * 5f);
                    _anchorBeacons.Add(beacon);
                }
            }

            for (int i = 0; i < _anchorBeacons.Count; i++)
            {
                LineRenderer beacon = _anchorBeacons[i];
                beacon.enabled = (_anchors[i] - local).sqrMagnitude < ChainViewRange * ChainViewRange;
                beacon.startColor = beacon.endColor = _climb != null && i <= _climb.BestAnchor ? LitColor : IronColor;
            }
        }

        private void ShowGust(WindGustPhase phase, Vector2 push)
        {
            if (phase == WindGustPhase.Calm)
            {
                if (_gustLine != null)
                {
                    _gustLine.enabled = false;
                }

                return;
            }

            _gustLine = _gustLine != null ? _gustLine : NewLine("Nartes_Gust", 0.18f);
            _gustLine.enabled = true;
            _gustLine.startColor = _gustLine.endColor = phase == WindGustPhase.Warning ? WarnColor : GustColor;
            Vector3 p = _player.position + Vector3.up * 1.2f;
            Vector3 d = new Vector3(push.x, 0f, push.y);
            _gustLine.SetPosition(0, p - d);
            _gustLine.SetPosition(1, p + d);
        }

        // ------------------------------------------------------------------
        // The Golds of Heaven
        // ------------------------------------------------------------------

        private void TickGolds(Vector2 local, long today)
        {
            Vector2[] golds = OverworldLayouts.NartesSkyGolds;
            ShowGolds(local);
            int here = -1;
            for (int i = 0; i < golds.Length; i++)
            {
                if (Vector2.Distance(golds[i], local) <= NartesSkyGoldRules.Reach)
                {
                    here = i;
                    break;
                }
            }

            if (here < 0)
            {
                _hintGold = -1;
                return;
            }

            string key = GoldKey(here);
            MapRequirementState state = _heroState.Get(_bag);
            NartesSkyGoldCheck check = NartesSkyGoldRules.Check(state.Hero, LastDay(key), today);
            ServiceLocator.TryGet(out LocalizationManager localization);
            string name = NartesMechanicsTexts.Get(localization, NartesMechanicsTexts.Golds[here]);
            if (_hintGold != here || Time.unscaledTime >= _nextHint)
            {
                _hintGold = here;
                _nextHint = Time.unscaledTime + HintCooldown;
                string text = check == NartesSkyGoldCheck.Take ? NartesMechanicsTexts.GoldCall : check == NartesSkyGoldCheck.Cooled ? NartesMechanicsTexts.GoldCooled : NartesMechanicsTexts.GoldHot;
                SayText(NartesMechanicsTexts.Format(localization, text, name));
            }

            if (check == NartesSkyGoldCheck.Take)
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
            }

            if (check == NartesSkyGoldCheck.Take && _controller.Input.InteractPressedThisFrame && ZoneOpen(local) && Pay(NartesSkyGoldRules.Purse))
            {
                SetDay(key, today);
                EventBus.Publish(new RegionFeatEvent(RegionFeats.NartesSkyGold(here)));
                SayText(NartesMechanicsTexts.Format(localization, NartesMechanicsTexts.GoldTaken, NartesSkyGoldRules.Purse));
            }
        }

        /// <summary>The claim key of Gold <paramref name="index"/> ("gold" + index), built once instead of per frame.</summary>
        private string GoldKey(int index)
        {
            if (_goldKeys == null)
            {
                _goldKeys = new string[OverworldLayouts.NartesSkyGolds.Length];
                for (int i = 0; i < _goldKeys.Length; i++)
                {
                    _goldKeys[i] = GoldDayKeyPrefix + i;
                }
            }

            return _goldKeys[index];
        }

        private void ShowGolds(Vector2 local)
        {
            Vector2[] golds = OverworldLayouts.NartesSkyGolds;
            if (_goldBeacons.Count == 0)
            {
                foreach (Vector2 gold in golds)
                {
                    LineRenderer beacon = NewLine("Nartes_SkyGold", 0.45f);
                    Vector3 foot = Ground(gold);
                    beacon.startColor = beacon.endColor = GoldColor;
                    beacon.SetPosition(0, foot);
                    beacon.SetPosition(1, foot + Vector3.up * 3f);
                    _goldBeacons.Add(beacon);
                }
            }

            for (int i = 0; i < golds.Length; i++)
            {
                _goldBeacons[i].enabled = (golds[i] - local).sqrMagnitude < GoldViewRange * GoldViewRange;
            }
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world)), world.y);
        }

        private void HideFeedback()
        {
            HideLines(_anchorBeacons);
            HideLines(_goldBeacons);
            ShowStart(false);
            HideGate();
            ShowGust(WindGustPhase.Calm, Vector2.zero);
        }

        private static void HideLines(List<LineRenderer> lines)
        {
            foreach (LineRenderer line in lines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_NartesLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(NartesMechanicsTexts.Get(localization, key));
        }

        private void SayText(string text) => CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
    }
}
