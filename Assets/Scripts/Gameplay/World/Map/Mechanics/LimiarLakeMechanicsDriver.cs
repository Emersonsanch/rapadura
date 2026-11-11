using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the boat of the Lago do Limiar (19, <see cref="LimiarBoatRules"/>) and the fear of the dark of the Salão de Tages (20,
    /// <see cref="TagesDarkRules"/>). Added to the overworld root, idle unless the layout holds the Limiar region; it works only inside the two
    /// maps' bounds. [E] at a landing sails the lake (<see cref="MechanicsRide"/>: the controls come back on arrival, on death, if anything else moves
    /// the hero and in OnDisable). [E] at an unlit lamp of the hall lights it; the beasts round each lamp get short-lived effects on their
    /// BuffController (bold by a dark lamp, calm by a lit one), refreshed while they apply, so they end by themselves when this driver stops. Deals
    /// no damage; the moment each lamp was lit stays in the save (time marks).
    /// </summary>
    public sealed class LimiarLakeMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        private const string BoldSource = "tages_dark_bold";
        private const string CalmSource = "tages_dark_calm";
        private const float FeedbackHeight = 2.4f;
        private const float HintCooldown = 3f;
        private const float BoldHintCooldown = 40f;
        private const float EffectSeconds = 1.2f;
        private const float BeastPassSeconds = 0.5f;
        private const float BeastRange = 80f;
        private const float LandingHeight = 5f;
        private const float LampLitHeight = 6f;
        private const float LampOutHeight = 2.2f;

        private static readonly Color LandingColor = new Color(1f, 0.7f, 0.28f, 1f);
        private static readonly Color LampLitColor = new Color(1f, 0.78f, 0.35f, 1f);
        private static readonly Color LampOutColor = new Color(0.28f, 0.27f, 0.42f, 0.8f);

        private readonly StaticGroundCache _ground = new StaticGroundCache();
        private readonly MechanicsRide _ride = new MechanicsRide();

        private OverworldView _view;
        private OverworldLayout _layout;
        private Vector2 _origin;
        private Rect _hall;
        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private Material _material;
        private LineRenderer[] _landingLines;
        private LineRenderer[] _lampLines;
        private long[] _lit;
        private long[] _litLocal;
        private bool[] _wasBurning;

        private StatModifierDefinition _boldDamage;
        private StatModifierDefinition _boldSpeed;
        private StatModifierDefinition _calmDamage;

        private float _nextHint;
        private int _hintSpot = -1;
        private float _lastRide = float.NegativeInfinity;
        private int _rideTo = -1;
        private float _nextBeastPass;
        private float _nextBoldHint;
        private bool _inLake;
        private bool _inHall;

        private Rect[] _areas;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => _ride.Active;

        private void Start()
        {
            _view = GetComponent<OverworldView>();
            _layout = _view != null ? _view.Layout : null;
            OverworldZone anchor = _layout?.Zone(OverworldLayouts.LimiarAnchorZoneId);
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            _origin = anchor.Origin;
            _hall = OverworldLayouts.TagesHallBounds;

            // The hub keeps the driver on over the whole region (like the lamps' driver): Update itself idles outside the lake and the hall.
            _areas = new[] { MechanicsDriverHub.WorldArea(OverworldLayouts.LimiarBounds, _origin) };
            int lamps = OverworldLayouts.TagesLamps.Length;
            _lampLines = new LineRenderer[lamps];
            _lit = new long[lamps];
            _litLocal = new long[lamps];
            _wasBurning = new bool[lamps];
            _landingLines = new LineRenderer[2];
            _boldDamage = NewEffect(StatType.AttackDamage, TagesDarkRules.BoldDamageFactor);
            _boldSpeed = NewEffect(StatType.MoveSpeed, TagesDarkRules.BoldSpeedFactor);
            _calmDamage = NewEffect(StatType.AttackDamage, TagesDarkRules.CalmDamageFactor);
        }

        private static StatModifierDefinition NewEffect(StatType stat, float factor) => new StatModifierDefinition
        {
            displayName = "Tages",
            affectedStat = stat,
            application = ModifierApplication.PercentAdditive,
            value = factor - 1f,
            durationSeconds = EffectSeconds,
            isDebuff = factor < 1f,
        };

        private void OnDisable()
        {
            EndRide(false);
            LeaveLake();
            LeaveHall();
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

            if (_ride.Active)
            {
                TickRide();
                return;
            }

            Vector2 local = new Vector2(_player.position.x, _player.position.z) - _origin;
            bool acted = false;
            if (OverworldLayouts.LimiarLakeBounds.Contains(local))
            {
                _inLake = true;
                acted = TickLake(local);
            }
            else
            {
                LeaveLake();
            }

            if (_hall.Contains(local))
            {
                _inHall = true;
                acted |= TickHall(local);
            }
            else
            {
                LeaveHall();
            }

            if (!acted)
            {
                _hintSpot = -1;
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
            }

            return _controller != null && _stats != null;
        }

        private static long Now() => Math.Max(1L, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        // ------------------------------------------------------------------
        // 19 · The boat
        // ------------------------------------------------------------------

        /// <summary>True while the hero stands at a landing.</summary>
        private bool TickLake(Vector2 local)
        {
            Vector2[] course = OverworldLayouts.LimiarBoatCourse;
            ShowLandings(course);
            if (!_stats.IsAlive || _controller.Input == null)
            {
                return false;
            }

            int landing = LimiarBoatRules.LandingAt(course, local);
            if (landing < 0)
            {
                return false;
            }

            bool ready = LimiarBoatRules.CanSail(landing, Time.unscaledTime - _lastRide);
            int spot = 100 + landing;
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(ready ? LimiarLakeTexts.BoatReady : LimiarLakeTexts.BoatResting);
            }

            if (!ready)
            {
                return true;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                BeginRide(landing);
            }

            return true;
        }

        private void BeginRide(int landing)
        {
            Vector2[] course = RidePath.ToWorld(LimiarBoatRules.Course(OverworldLayouts.LimiarBoatCourse, landing), _origin, false);
            if (!_ride.Begin(_player.gameObject, _layout, course, LimiarBoatRules.Speed, 0f, LimiarBoatRules.Hover))
            {
                return;
            }

            _rideTo = 1 - landing;
            _hintSpot = -1;
            Say(LimiarLakeTexts.BoatSailing);
        }

        private void TickRide()
        {
            RideStep step = _ride.Tick(Time.deltaTime);
            if (step != RideStep.Riding)
            {
                EndRide(step == RideStep.Arrived);
            }
        }

        /// <summary>Hands the controls back (the ride already did, unless the driver is being switched off) and sets the hero on the far landing.</summary>
        private void EndRide(bool arrived)
        {
            _ride.End();
            if (_rideTo < 0)
            {
                return;
            }

            int to = _rideTo;
            _rideTo = -1;
            _lastRide = Time.unscaledTime;
            if (!arrived || _view == null || _player == null)
            {
                return;
            }

            Vector2[] course = OverworldLayouts.LimiarBoatCourse;
            Vector2 local = to == 0 ? course[0] : course[course.Length - 1];
            Vector2 before = to == 0 ? course[1] : course[course.Length - 2];
            Vector2 end = _origin + local;
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, local - before);
            _view.ResetTracking(end);
            Say(LimiarLakeTexts.BoatLanded);
        }

        private void ShowLandings(Vector2[] course)
        {
            Post(ref _landingLines[0], "Lm_BoatLanding", 0.5f, course[0], LandingHeight, LandingColor);
            Post(ref _landingLines[1], "Lm_BoatLanding", 0.5f, course[course.Length - 1], LandingHeight, LandingColor);
        }

        private void LeaveLake()
        {
            if (!_inLake)
            {
                return;
            }

            _inLake = false;
            Hide(_landingLines);
        }

        // ------------------------------------------------------------------
        // 20 · The lamps of the hall
        // ------------------------------------------------------------------

        private static bool LightBearer() => ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected == CharacterId.Maria;

        private void ReadMarks()
        {
            bool saved = ServiceLocator.TryGet(out WorldMapProgress progress);
            TagesLamp[] lamps = OverworldLayouts.TagesLamps;
            for (int i = 0; i < lamps.Length; i++)
            {
                _lit[i] = saved ? progress.GetTimeMark(TagesDarkRules.LitKey(lamps[i].Id)) : _litLocal[i];
            }
        }

        /// <summary>True while the hero stands at a lamp.</summary>
        private bool TickHall(Vector2 local)
        {
            long now = Now();
            ReadMarks();
            TagesLamp[] lamps = OverworldLayouts.TagesLamps;
            bool sighed = false;
            for (int i = 0; i < lamps.Length; i++)
            {
                bool burning = TagesDarkRules.Burning(_lit[i], now);
                sighed |= _wasBurning[i] && !burning;
                _wasBurning[i] = burning;
                Post(ref _lampLines[i], "Lm_TagesLamp", burning ? 0.6f : 0.4f, lamps[i].At, burning ? LampLitHeight : LampOutHeight, burning ? LampLitColor : LampOutColor);
            }

            if (sighed)
            {
                Say(LimiarLakeTexts.HallSighs);
            }

            TickBeasts(local, now);
            if (!_stats.IsAlive || _controller.Input == null)
            {
                return false;
            }

            int lamp = TagesDarkRules.LampAt(lamps, local, TagesDarkRules.ReachFor(LightBearer()));
            if (lamp < 0)
            {
                return false;
            }

            bool canLight = TagesDarkRules.CanLight(_lit[lamp], now);
            int spot = 200 + lamp;
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_hintSpot != spot || Time.unscaledTime >= _nextHint)
            {
                _hintSpot = spot;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayText(canLight
                    ? LimiarLakeTexts.Get(localization, LimiarLakeTexts.LampOut)
                    : LimiarLakeTexts.Format(localization, LimiarLakeTexts.LampBurning, Math.Max(1L, (TagesDarkRules.SecondsLeft(_lit[lamp], now) + 59L) / 60L)));
            }

            if (!canLight)
            {
                return true;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return true;
            }

            _litLocal[lamp] = now;
            _lit[lamp] = now;
            _wasBurning[lamp] = true;
            if (ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                progress.SetTimeMark(TagesDarkRules.LitKey(lamps[lamp].Id), now);
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            SayText(LimiarLakeTexts.Format(localization, LimiarLakeTexts.LampLit, TagesDarkRules.BurningCount(_lit, now), lamps.Length));
            _nextBeastPass = 0f;
            return true;
        }

        /// <summary>Sets the mood of the beasts round the lamps: bold by an unlit one, calm by a lit one (bosses left alone).</summary>
        private void TickBeasts(Vector2 local, long now)
        {
            if (Time.time < _nextBeastPass)
            {
                return;
            }

            _nextBeastPass = Time.time + BeastPassSeconds;
            TagesLamp[] lamps = OverworldLayouts.TagesLamps;
            bool boldNear = false;
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy == null || enemy.IsBoss || enemy.Buffs == null || enemy.Health == null || !enemy.Health.IsAlive)
                {
                    continue;
                }

                Vector2 at = new Vector2(enemy.transform.position.x, enemy.transform.position.z) - _origin;
                if ((at - local).sqrMagnitude > BeastRange * BeastRange || !_hall.Contains(at))
                {
                    continue;
                }

                switch (TagesDarkRules.MoodAt(lamps, _lit, at, now))
                {
                    case TagesBeastMood.Bold:
                        enemy.Buffs.RemoveEffectsBySource(CalmSource);
                        enemy.Buffs.ApplyEffect(_boldDamage, BoldSource);
                        enemy.Buffs.ApplyEffect(_boldSpeed, BoldSource);
                        boldNear |= (at - local).sqrMagnitude <= TagesDarkRules.BeastReach * TagesDarkRules.BeastReach;
                        break;
                    case TagesBeastMood.Calm:
                        enemy.Buffs.RemoveEffectsBySource(BoldSource);
                        enemy.Buffs.ApplyEffect(_calmDamage, CalmSource);
                        break;
                }
            }

            if (boldNear && Time.unscaledTime >= _nextBoldHint)
            {
                _nextBoldHint = Time.unscaledTime + BoldHintCooldown;
                Say(LimiarLakeTexts.BeastsBold);
            }
        }

        private void LeaveHall()
        {
            if (!_inHall)
            {
                return;
            }

            _inHall = false;
            Hide(_lampLines);
            if (_wasBurning != null)
            {
                Array.Clear(_wasBurning, 0, _wasBurning.Length);
            }
        }

        // ------------------------------------------------------------------
        // Shown in the scene
        // ------------------------------------------------------------------

        private void Post(ref LineRenderer line, string objectName, float width, Vector2 at, float height, Color color)
        {
            if (line == null)
            {
                line = NewLine(objectName, width);
            }

            Vector2 world = _origin + at;
            Vector3 foot = new Vector3(world.x, _ground.GroundAt(_layout, world), world.y);
            line.enabled = true;
            line.startWidth = line.endWidth = width;
            line.startColor = line.endColor = color;
            line.SetPosition(0, foot);
            line.SetPosition(1, foot + Vector3.up * height);
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
                _material = new Material(shader) { name = "Mat_LimiarLakeLine_Runtime" };
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
            SayText(LimiarLakeTexts.Get(localization, key));
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
