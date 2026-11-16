using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Bomba de Água (68, <see cref="WaterBombRules"/>) on the Hinokura driver: when the hero uses one from the bag (<see cref="ItemConsumedEvent"/>)
    /// it bursts at their feet onto <see cref="WaterBombBoard"/>, which the turtle's fight (its wake), the eruption (67) and the heat (70) read.
    /// A blue ring shows the crust while it lasts. Nothing is saved: a splash lives 20 s.
    /// </summary>
    public sealed partial class HinokuraMechanicsDriver
    {
        private const float SplashLift = 0.2f;
        private const int SplashPoints = 33;
        private const float TurtleNear = 30f;

        private static readonly Color SplashColor = new Color(0.45f, 0.78f, 1f, 0.95f);

        private LineRenderer _splashRing;
        private double _splashUntil = double.NegativeInfinity;
        private bool _splashLineSaid;

        private void OnEnable() => EventBus.Subscribe<ItemConsumedEvent>(OnItemConsumed);

        private void UnhookWaterBomb()
        {
            EventBus.Unsubscribe<ItemConsumedEvent>(OnItemConsumed);
            if (_splashRing != null)
            {
                _splashRing.enabled = false;
            }
        }

        /// <summary>Is the hero's spot (region-local) under a live splash?</summary>
        private bool SplashHolds(Vector2 local) => WaterBombBoard.Covers(_origin + local, Time.timeAsDouble);

        private void OnItemConsumed(ItemConsumedEvent evt)
        {
            if (!ConsumableEffectRules.ThrowsWater(evt.ItemId) || _player == null || evt.User == null
                || (evt.User.transform != _player && !evt.User.transform.IsChildOf(_player)))
            {
                return;
            }

            Vector2 flat = new Vector2(_player.position.x, _player.position.z);
            double now = Time.timeAsDouble;
            WaterBombBoard.Splash(flat, now);
            _splashUntil = now + WaterBombRules.Seconds;
            ShowSplash(flat);

            Vector2 local = flat - _origin;
            ServiceLocator.TryGet(out LocalizationManager localization);
            string key = SplashKey(local);
            if (key == WaterBombTexts.NoLava)
            {
                SayText(WaterBombTexts.Get(localization, key));
                return;
            }

            SayText(WaterBombTexts.Format(localization, key, Mathf.RoundToInt(WaterBombRules.Seconds)));
            if (!_splashLineSaid)
            {
                _splashLineSaid = true;
                string line = WaterBombTexts.LineFor(PuzzleHero());
                if (line != null)
                {
                    SayText(WaterBombTexts.Get(localization, line));
                }
            }
        }

        /// <summary>What the splash at <paramref name="local"/> hardened: the turtle's wake, the eruption's flow, the caldera's heat or nothing.</summary>
        private string SplashKey(Vector2 local)
        {
            if (!OverworldLayouts.HinokuraBounds.Contains(local))
            {
                return WaterBombTexts.NoLava;
            }

            if ((local - OverworldLayouts.TartarugaVulcaoArena).sqrMagnitude <= TurtleNear * TurtleNear)
            {
                return WaterBombTexts.Wake;
            }

            long unix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (HinokuraEruptionRules.PhaseAt(unix) != HinokuraEruptionPhase.Calm
                && HinokuraEruptionRules.InRoute(OverworldLayouts.EruptionRoutes, HinokuraEruptionRules.RouteAt(unix), local))
            {
                return WaterBombTexts.Eruption;
            }

            if (RioFireRules.DistanceToFire(OverworldLayouts.CaldeiraLake, local) < HinokuraHeatRules.HeatRange)
            {
                return WaterBombTexts.Heat;
            }

            return WaterBombTexts.NoLava;
        }

        /// <summary>Hides the ring when the splash dried (called every frame from <c>Update</c>).</summary>
        private void TickWaterBomb()
        {
            if (_splashRing == null || !_splashRing.enabled || Time.timeAsDouble < _splashUntil)
            {
                return;
            }

            _splashRing.enabled = false;
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(WaterBombTexts.Get(localization, WaterBombTexts.Dried));
        }

        private void ShowSplash(Vector2 flat)
        {
            _splashRing = _splashRing != null ? _splashRing : NewLine("Hinokura_WaterSplash", 0.18f);
            _splashRing.loop = true;
            _splashRing.startColor = _splashRing.endColor = SplashColor;
            _splashRing.positionCount = SplashPoints;
            float y = _player.position.y + SplashLift;
            for (int i = 0; i < SplashPoints; i++)
            {
                float a = i * Mathf.PI * 2f / SplashPoints;
                _splashRing.SetPosition(i, new Vector3(flat.x + Mathf.Cos(a) * WaterBombRules.Radius, y, flat.y + Mathf.Sin(a) * WaterBombRules.Radius));
            }

            _splashRing.enabled = true;
        }
    }
}
