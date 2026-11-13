using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
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
    /// Scene side of the last capital events (Docs/Mapas100.md): A Barca Atrasou of the Cais de Mesektet (71, <see cref="MesektetLateBarqueRules"/>,
    /// FinalCityEventsMechanicsDriver.Barque.cs) with the river's everlasting night (<see cref="RioEverNightRules"/>), A Cheia of Dur-Jarras and the
    /// ziggurat's steps (77, <see cref="DurJarrasFloodRules"/>, FinalCityEventsMechanicsDriver.Flood.cs), the Chegada da Caravana of the Cais da
    /// Coluna with its low gravity (91, <see cref="CaisCaravanRules"/>, <see cref="CeuLowGravityRules"/>, FinalCityEventsMechanicsDriver.Caravan.cs)
    /// and the pay of the Maré de Treva of the Fortaleza da Última Lâmpada (83, <see cref="ProfundoTideRewardRules"/>,
    /// FinalCityEventsMechanicsDriver.World.cs). Same shape as <see cref="LateCityEventsMechanicsDriver"/>: added to the overworld root; each part
    /// is idle unless the layout holds its region, and runs only near its town (or in its region, for the night and the gravity); OnDisable puts
    /// out everything it showed, sends back the scarabs it raised, gives the sky back to the clock and the hero back his weight. Damage to the
    /// hero only through <see cref="PlayerStats.ApplyHazardDamage"/> (the flood, never kills); the paid marks stay in the save
    /// (<see cref="WorldMapProgress"/> time marks).
    /// </summary>
    public sealed partial class FinalCityEventsMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float FeedbackHeight = 2.4f;
        private const string GoldItemId = "item_gold";

        /// <summary>Half side (m) of the square round each town where the driver runs (the hub adds its own margin).</summary>
        public const float TownArea = 280f;

        /// <summary>Half side (m) round the Fortaleza: the Tide of Darkness reaches 320 m from its plaza.</summary>
        public const float FortressArea = 330f;

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private OverworldLayout _layout;
        private OverworldView _view;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private PlayerMotor _motor;
        private InventoryManager _bag;
        private Material _material;
        private Rect[] _areas;
        private CharacterId? _hero;
        private float _heroReadAt;

        private bool _hasMesektet;
        private bool _hasDurJarras;
        private bool _hasColuna;
        private bool _hasFortress;
        private Vector2 _mesektetOrigin;
        private Vector2 _durJarrasOrigin;
        private Vector2 _colunaOrigin;
        private Vector2 _fortressOrigin;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            var areas = new List<Rect>();
            _hasMesektet = TryOrigin(OverworldLayouts.RioAnchorZoneId, OverworldLayouts.CaisMesektetOffset, TownArea, areas, out _mesektetOrigin);
            if (_hasMesektet)
            {
                // The everlasting night covers the whole river.
                areas.Add(MechanicsDriverHub.WorldArea(OverworldLayouts.RioBounds, _mesektetOrigin));
            }

            _hasDurJarras = TryOrigin(OverworldLayouts.MisturaAnchorZoneId, OverworldLayouts.DurJarrasOffset, TownArea, areas, out _durJarrasOrigin);
            _hasColuna = TryOrigin(OverworldLayouts.CeuAnchorZoneId, OverworldLayouts.CaisDaColunaOffset, TownArea, areas, out _colunaOrigin);
            if (_hasColuna)
            {
                // The low gravity covers the whole of map 91.
                areas.Add(MechanicsDriverHub.WorldArea(CeuLowGravityRules.Map91, _colunaOrigin));
            }

            _hasFortress = TryOrigin(OverworldLayouts.ProfundoAnchorZoneId, OverworldLayouts.FortalezaUltimaLampadaOffset, FortressArea, areas, out _fortressOrigin);
            if (areas.Count == 0)
            {
                enabled = false;
                return;
            }

            _areas = areas.ToArray();
        }

        private bool TryOrigin(string anchorZoneId, Vector2 town, float half, List<Rect> areas, out Vector2 origin)
        {
            OverworldZone anchor = _layout?.Zone(anchorZoneId);
            origin = anchor != null ? anchor.Origin : Vector2.zero;
            if (anchor == null)
            {
                return false;
            }

            Vector2 world = origin + town;
            areas.Add(Rect.MinMaxRect(world.x - half, world.y - half, world.x + half, world.y + half));
            return true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyDiedEvent>(HandleEnemyDied);
            EventBus.Subscribe<ProfundoTideEndedEvent>(HandleTideEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyDiedEvent>(HandleEnemyDied);
            EventBus.Unsubscribe<ProfundoTideEndedEvent>(HandleTideEnded);
            ClearScarabs();
            HideBarque();
            HideFlood();
            HideCaravan();
            ReleaseWorld();
        }

        private void OnDestroy()
        {
            ClearScarabs();
            ReleaseWorld();
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
            if (_hasMesektet)
            {
                TickEverNight(world - _mesektetOrigin);
                TickBarque(world - _mesektetOrigin, utc);
            }

            if (_hasDurJarras)
            {
                TickFlood(world - _durJarrasOrigin, utc);
            }

            if (_hasColuna)
            {
                TickLowGravity(world - _colunaOrigin);
                TickCaravan(world - _colunaOrigin, utc);
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
                _motor = go.GetComponent<PlayerMotor>();
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

        /// <summary>The cousin playing now (null when the bag cannot tell), read from the bag at most once a second.</summary>
        private CharacterId? Hero()
        {
            if (Time.unscaledTime >= _heroReadAt)
            {
                _heroReadAt = Time.unscaledTime + 1f;
                _hero = MapRequirementHeroState.Capture(_bag).Hero;
            }

            return _hero;
        }

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

                    Say(FinalCityEventsTexts.BagFull);
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

                    Say(FinalCityEventsTexts.BagFull);
                    return false;
                }
            }

            return true;
        }

        private void CousinLine(string kind)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = FinalCityEventsTexts.LineFor(kind, Hero());
            SayText(FinalCityEventsTexts.Get(localization, key), 0.7f);
        }

        private string SideName(int side)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            return FinalCityEventsTexts.Get(localization, FinalCityEventsTexts.SideKey(side));
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
                line = NewLine(objectName, width, 2);
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

        private LineRenderer NewLine(string objectName, float width, int points)
        {
            if (_material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "Mat_FinalCityEventsLine_Runtime" };
            }

            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = _material;
            line.useWorldSpace = true;
            line.positionCount = points;
            line.startWidth = line.endWidth = width;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
        }

        private void Say(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(FinalCityEventsTexts.Get(localization, key));
        }

        private void SayFormat(string key, params object[] arguments)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(FinalCityEventsTexts.Format(localization, key, arguments));
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
