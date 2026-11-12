using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the three capital events (Docs/Mapas100.md): the Maré de Lodo of Jhilpur (31, <see cref="JhilpurMudTideRules"/>,
    /// CityEventsMechanicsDriver.Tide.cs), the Avalanche of Sangkent (36, <see cref="SangkentAvalancheRules"/>, CityEventsMechanicsDriver.Snow.cs)
    /// and the Festa das Lanternas of Mingcheng (41, <see cref="MingchengLanternFestaRules"/>, CityEventsMechanicsDriver.Festa.cs). Added to the
    /// overworld root; each event is idle unless the layout holds its region, and runs only near its town (the hub switches the driver off
    /// elsewhere, and OnDisable puts out everything it showed and sends back the Filhos it raised). Damage to the hero only through
    /// <see cref="PlayerStats.ApplyHazardDamage"/> (the snow front, never kills); the paid marks stay in the save (<see cref="WorldMapProgress"/> time marks).
    /// </summary>
    public sealed partial class CityEventsMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float FeedbackHeight = 2.4f;
        private const string GoldItemId = "item_gold";

        /// <summary>Half side (m) of the square round each town where the driver runs (the hub adds its own margin).</summary>
        public const float TownArea = 280f;

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private OverworldLayout _layout;
        private OverworldView _view;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private Rect[] _areas;

        private bool _hasDelta;
        private bool _hasTeto;
        private bool _hasMingcheng;
        private Vector2 _deltaOrigin;
        private Vector2 _tetoOrigin;
        private Vector2 _mingchengOrigin;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            var areas = new List<Rect>();
            _hasDelta = TryOrigin(OverworldLayouts.DeltaAnchorZoneId, OverworldLayouts.JhilpurOffset, areas, out _deltaOrigin);
            _hasTeto = TryOrigin(OverworldLayouts.TetoAnchorZoneId, OverworldLayouts.SangkentOffset, areas, out _tetoOrigin);
            _hasMingcheng = TryOrigin(OverworldLayouts.MingchengAnchorZoneId, OverworldLayouts.MingchengOffset, areas, out _mingchengOrigin);
            if (areas.Count == 0)
            {
                enabled = false;
                return;
            }

            _areas = areas.ToArray();
        }

        private bool TryOrigin(string anchorZoneId, Vector2 town, List<Rect> areas, out Vector2 origin)
        {
            OverworldZone anchor = _layout?.Zone(anchorZoneId);
            origin = anchor != null ? anchor.Origin : Vector2.zero;
            if (anchor == null)
            {
                return false;
            }

            Vector2 world = origin + town;
            areas.Add(Rect.MinMaxRect(world.x - TownArea, world.y - TownArea, world.x + TownArea, world.y + TownArea));
            return true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(HandleEnemyDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(HandleEnemyDied);
            ClearFilhos();
            HideSnow();
            HideFesta();
        }

        private void OnDestroy()
        {
            ClearFilhos();
            if (_material != null)
            {
                Destroy(_material);
            }
        }

        private void Update()
        {
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            Vector2 world = new Vector2(_player.position.x, _player.position.z);
            DateTime utc = WorldEventCalendar.UtcNow();
            if (_hasDelta)
            {
                TickTide(world - _deltaOrigin, utc);
            }

            if (_hasTeto)
            {
                TickSnow(world - _tetoOrigin, utc);
            }

            if (_hasMingcheng)
            {
                TickFesta(world - _mingchengOrigin);
            }
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
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
            }

            return _controller != null && _stats != null;
        }

        private static long GetMark(string key, long local) => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.GetTimeMark(key) : local;

        private static void SetMark(string key, long value)
        {
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(key, value);
            }
        }

        private bool InteractPressed() => _controller != null && _controller.Input != null && _controller.Input.InteractPressedThisFrame;

        /// <summary>Gold and Pedras-Faísca into the bag; false (nothing given) when they do not fit.</summary>
        private bool Grant(int gold, int stones)
        {
            if (_bag == null)
            {
                return false;
            }

            ItemDefinition goldItem = gold > 0 ? ItemDatabase.GetById(GoldItemId) : null;
            ItemDefinition stoneItem = stones > 0 ? ItemDatabase.GetById(EventRoomRules.BlessingStoneItemId) : null;
            if ((gold > 0 && goldItem == null) || (stones > 0 && stoneItem == null))
            {
                return false;
            }

            if (goldItem != null)
            {
                int left = _bag.AddItem(goldItem, gold);
                if (left > 0)
                {
                    if (gold - left > 0)
                    {
                        _bag.RemoveById(GoldItemId, gold - left);
                    }

                    Say(CityEventsTexts.BagFull);
                    return false;
                }
            }

            if (stoneItem != null)
            {
                int left = _bag.AddItem(stoneItem, stones);
                if (left > 0)
                {
                    // All or nothing: take back what went in, so the reward stays due and is paid when there is room.
                    if (stones - left > 0)
                    {
                        _bag.RemoveById(EventRoomRules.BlessingStoneItemId, stones - left);
                    }

                    if (goldItem != null)
                    {
                        _bag.RemoveById(GoldItemId, gold);
                    }

                    Say(CityEventsTexts.BagFull);
                    return false;
                }
            }

            return true;
        }

        private void CousinLine(string kind)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = CityEventsTexts.LineFor(kind, MapRequirementHeroState.Capture(_bag).Hero);
            SayText(CityEventsTexts.Get(localization, key), 0.7f);
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private Vector3 Ground(Vector2 world) => new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);

        private void Beam(ref LineRenderer line, string objectName, float width, Vector2 world, float height, Color color)
        {
            Vector3 foot = Ground(world);
            Post(ref line, objectName, width, foot, foot + Vector3.up * height, color);
        }

        private void Post(ref LineRenderer line, string objectName, float width, Vector3 from, Vector3 to, Color color)
        {
            if (line == null)
            {
                line = NewLine(objectName, width);
            }

            line.enabled = true;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }

        private static void Hide(LineRenderer line)
        {
            if (line != null)
            {
                line.enabled = false;
            }
        }

        private static void Hide(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (LineRenderer line in lines)
            {
                Hide(line);
            }
        }

        private LineRenderer NewLine(string objectName, float width)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_CityEventsLine_Runtime" };
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
            SayText(CityEventsTexts.Get(localization, key));
        }

        private void SayFormat(string key, params object[] arguments)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(CityEventsTexts.Format(localization, key, arguments));
        }

        private void SayText(string text, float lift = 0f)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), text);
            }
        }
    }
}
