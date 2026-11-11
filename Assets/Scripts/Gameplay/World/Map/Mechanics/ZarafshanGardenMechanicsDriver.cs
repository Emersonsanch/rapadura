using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the sparks and canals of the Jardim de Mihrshah (06, <see cref="ZarafshanGardenRules"/>): [E] at a quadrant's cage frees
    /// its spark once no beast guards it, the freed spark fills one arm of the cross of canals (a turquoise line along it) and a lit arm mends the
    /// hero out of combat; with the four free, the pavilion mends life and mana. Added to the overworld root, idle unless the layout holds the
    /// Zarafshan region; it works only inside the garden's walls. Never takes the controls and deals no damage; the freed sparks stay in the
    /// save (time marks).
    /// </summary>
    public sealed class ZarafshanGardenMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const float HintCooldown = 3f;
        private const float MendSayCooldown = 25f;
        private const float MendTickSeconds = 1f;
        private const float FeedbackHeight = 2.4f;
        private const float CageHeight = 6f;
        private const float FreeHeight = 12f;
        private const float CanalLift = 0.15f;

        /// <summary>The walled garden around its centre (region coordinates are added at Start): where the driver does anything.</summary>
        public const float GardenHalf = 62f;

        private static readonly Color CaughtColor = new Color(1f, 0.55f, 0.35f, 1f);
        private static readonly Color GuardedColor = new Color(0.7f, 0.35f, 0.3f, 0.8f);
        private static readonly Color FreeColor = new Color(1f, 0.92f, 0.6f, 0.9f);
        private static readonly Color CanalColor = new Color(0.3f, 0.85f, 0.8f, 0.95f);

        private readonly HeroCombatClock _combat = new HeroCombatClock();
        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Rect _garden;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private Material _material;
        private LineRenderer[] _cages;
        private LineRenderer[] _canals;
        private long[] _freed;
        private long[] _freedLocal;

        private float _nextHint;
        private int _hintSpot = -1;
        private float _nextMendSay;
        private float _mendTick;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            _layout = view != null ? view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.ZarafshanAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            Vector2 c = OverworldLayouts.MihrshahGardenCentre;
            _garden = Rect.MinMaxRect(c.x - GardenHalf, c.y - GardenHalf, c.x + GardenHalf, c.y + GardenHalf);
            // The hub keeps the driver on over the whole region (like the rafts' driver): Update itself idles outside the garden's walls.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.ZarafshanBounds, _origin) };
            int n = OverworldLayouts.MihrshahSparks.Length;
            _cages = new LineRenderer[n];
            _canals = new LineRenderer[n];
            _freed = new long[n];
            _freedLocal = new long[n];
        }

        private void OnEnable() => _combat.Attach();

        private void OnDisable()
        {
            _combat.Detach();
            Hide(_cages);
            Hide(_canals);
            _hintSpot = -1;
            _mendTick = 0f;
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
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            if (!_garden.Contains(local))
            {
                Hide(_cages);
                Hide(_canals);
                _hintSpot = -1;
                _mendTick = 0f;
                return;
            }

            ReadMarks();
            Show();
            if (!_stats.IsAlive)
            {
                return;
            }

            TickCages(local);
            TickMend(local);
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
                _combat.Track(go);
            }

            return _controller != null && _stats != null;
        }

        private void ReadMarks()
        {
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            for (int i = 0; i < sparks.Length; i++)
            {
                _freed[i] = saved ? progress.GetTimeMark(ZarafshanGardenRules.FreedKey(sparks[i].Id)) : _freedLocal[i];
            }
        }

        // ------------------------------------------------------------------
        // The cages
        // ------------------------------------------------------------------

        private int GuardsAt(Vector2 cageLocal)
        {
            int guards = 0;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector3 p = enemy.transform.position;
                if (ZarafshanGardenRules.Guards(cageLocal, new Vector2(p.x, p.z) - _origin))
                {
                    guards++;
                }
            }

            return guards;
        }

        private void TickCages(Vector2 local)
        {
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            int cage = ZarafshanGardenRules.CageAt(sparks, local);
            if (cage < 0 || _controller.Input == null)
            {
                _hintSpot = -1;
                return;
            }

            int guards = ZarafshanGardenRules.IsFree(_freed[cage]) ? 0 : GuardsAt(sparks[cage].Cage);
            ZarafshanSparkCheck check = ZarafshanGardenRules.Check(_freed[cage], guards);
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_hintSpot != cage || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = cage;
                _nextHint = Time.unscaledTime + HintCooldown;
                switch (check)
                {
                    case ZarafshanSparkCheck.Free:
                        SayText(ZarafshanGardenTexts.Get(localization, ZarafshanGardenTexts.CageFree));
                        break;
                    case ZarafshanSparkCheck.Guarded:
                        SayText(ZarafshanGardenTexts.Format(localization, ZarafshanGardenTexts.CageGuarded, guards));
                        break;
                    default:
                        SayText(ZarafshanGardenTexts.Get(localization, ZarafshanGardenTexts.CageDone));
                        break;
                }
            }

            if (check != ZarafshanSparkCheck.Free)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            long now = Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            _freedLocal[cage] = now;
            _freed[cage] = now;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(ZarafshanGardenRules.FreedKey(sparks[cage].Id), now);
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            int free = ZarafshanGardenRules.FreeCount(_freed);
            if (ZarafshanGardenRules.PavilionOpen(_freed, sparks.Length))
            {
                SayText(ZarafshanGardenTexts.Get(localization, ZarafshanGardenTexts.AllFreed));
                CharacterLine(localization);
            }
            else
            {
                SayText(ZarafshanGardenTexts.Format(localization, ZarafshanGardenTexts.Freed, free, sparks.Length));
            }
        }

        private void CharacterLine(LocalizationManager localization)
        {
            string key = ZarafshanGardenTexts.LineFor(MapRequirementHeroState.Capture(_bag).Hero);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), ZarafshanGardenTexts.Get(localization, key));
        }

        // ------------------------------------------------------------------
        // The lit water
        // ------------------------------------------------------------------

        private void TickMend(Vector2 local)
        {
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            bool pavilion = ZarafshanGardenRules.PavilionOpen(_freed, sparks.Length) && ZarafshanGardenRules.InPavilion(OverworldLayouts.MihrshahGardenCentre, local);
            bool canal = !pavilion && ZarafshanGardenRules.LitCanalAt(sparks, _freed, local) >= 0;
            if (!pavilion && !canal)
            {
                _mendTick = 0f;
                return;
            }

            bool hurt = _stats.CurrentHealth < _stats.MaxHealth || (pavilion && _stats.CurrentMana < _stats.MaxMana);
            if (!ZarafshanGardenRules.OutOfCombat(_combat.SecondsSinceCombat))
            {
                _mendTick = 0f;
                if (hurt && Time.unscaledTime >= _nextMendSay)
                {
                    _nextMendSay = Time.unscaledTime + MendSayCooldown;
                    Say(ZarafshanGardenTexts.CanalWaits);
                }

                return;
            }

            _mendTick += Time.deltaTime;
            if (_mendTick < MendTickSeconds)
            {
                return;
            }

            float seconds = _mendTick;
            _mendTick = 0f;
            if (!hurt)
            {
                return;
            }

            if (Time.unscaledTime >= _nextMendSay)
            {
                _nextMendSay = Time.unscaledTime + MendSayCooldown;
                Say(pavilion ? ZarafshanGardenTexts.PavilionMends : ZarafshanGardenTexts.CanalMends);
            }

            float lifeRate = pavilion ? ZarafshanGardenRules.PavilionHealPerSecond : ZarafshanGardenRules.CanalHealPerSecond;
            float heal = ZarafshanGardenRules.MendOver(_stats.CurrentHealth, _stats.MaxHealth, lifeRate, seconds);
            if (heal > 0f)
            {
                _stats.Heal(heal);
            }

            if (pavilion)
            {
                float mana = ZarafshanGardenRules.MendOver(_stats.CurrentMana, _stats.MaxMana, ZarafshanGardenRules.PavilionManaPerSecond, seconds);
                if (mana > 0f)
                {
                    _stats.RestoreMana(mana);
                }
            }
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private void Show()
        {
            ZarafshanGardenSpark[] sparks = OverworldLayouts.MihrshahSparks;
            for (int i = 0; i < sparks.Length; i++)
            {
                bool free = ZarafshanGardenRules.IsFree(_freed[i]);
                Color cageColor = free ? FreeColor : (GuardsAt(sparks[i].Cage) > 0 ? GuardedColor : CaughtColor);
                Vector3 foot = Ground(sparks[i].Cage);
                Post(ref _cages[i], "Zf_GardenSpark", 0.6f, foot, foot + Vector3.up * (free ? FreeHeight : CageHeight), cageColor);
                if (free)
                {
                    Post(ref _canals[i], "Zf_GardenCanal", 1.6f, Ground(sparks[i].CanalA) + Vector3.up * CanalLift, Ground(sparks[i].CanalB) + Vector3.up * CanalLift, CanalColor);
                }
                else if (_canals[i] != null)
                {
                    _canals[i].enabled = false;
                }
            }
        }

        private Vector3 Ground(Vector2 local)
        {
            Vector2 world = _origin + local;
            return new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
        }

        private void Post(ref LineRenderer line, string objectName, float width, Vector3 from, Vector3 to, Color color)
        {
            if (line == null)
            {
                line = NewLine(objectName, width);
            }

            line.enabled = true;
            line.startColor = line.endColor = color;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
        }

        private static void Hide(LineRenderer[] lines)
        {
            if (lines == null)
            {
                return;
            }

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
                _material = new Material(shader) { name = "Mat_ZarafshanGardenLine_Runtime" };
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
            SayText(ZarafshanGardenTexts.Get(localization, key));
        }

        private void SayText(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
