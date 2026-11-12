using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The fog of the Copas de Garajonay (05, <see cref="GarajonayFogRules"/>), on the same driver as the mud and the webs: in the fog, on the
    /// forest floor, the beasts past 8 m are lost to sight through a <see cref="BeastSightHider"/> (every one shows again when the fog lifts,
    /// the hero climbs above it, leaves the map or the driver stops); pale streaks of mist circle the hero. A fog crossed earns a drop that the
    /// Coletor de Névoa hands over as an herb the next time the hero stands in the city; the counts stay in the save (time marks).
    /// </summary>
    public sealed partial class ZarafshanMireMechanicsDriver
    {
        private const int MistStreaks = 8;
        private const float FogPaySeconds = 1f;

        private static readonly Color MistColor = new Color(0.9f, 0.93f, 0.95f, 0.55f);

        private readonly BeastSightHider _fogSight = new BeastSightHider();
        private readonly LineRenderer[] _mist = new LineRenderer[MistStreaks];

        private InventoryManager _fogBag;
        private bool _inCanopy;
        private GarajonayFogPhase _fogPhase;
        private long _fogIndex = long.MinValue;
        private float _fogSeconds;
        private bool _fogEarned;
        private bool _aboveSaid;
        private bool _fogUpSaid;
        private float _nextFogPay;

        private void TickFog(Vector2 local)
        {
            _inCanopy = true;
            Vector2 world = _origin + local;
            double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000d;
            GarajonayFogPhase phase = GarajonayFogRules.PhaseAt(now);
            long index = GarajonayFogRules.FogIndex(now);
            bool inCity = OverworldRules.SettlementAt(_layout, world) != null;
            bool alive = _stats.IsAlive;
            float height = phase == GarajonayFogPhase.Fog ? _player.position.y - OverworldRules.SurfaceAt(_layout, world) : 0f;
            float sight = GarajonayFogRules.SightFor(phase, height, inCity, alive);

            if (phase != _fogPhase)
            {
                if (phase == GarajonayFogPhase.Rising && !inCity)
                {
                    Say(RouteSightAirColdTexts.FogRising, RouteSightAirColdTexts.Get);
                }

                _fogPhase = phase;
                _aboveSaid = false;
                _fogUpSaid = false;
            }

            if (phase == GarajonayFogPhase.Fog && !inCity && alive)
            {
                if (BeastSightRules.Active(sight) && !_fogUpSaid)
                {
                    _fogUpSaid = true;
                    Say(RouteSightAirColdTexts.FogUp, RouteSightAirColdTexts.Get);
                }
                else if (!BeastSightRules.Active(sight) && !_aboveSaid)
                {
                    _aboveSaid = true;
                    Say(RouteSightAirColdTexts.FogAbove, RouteSightAirColdTexts.Get);
                }

                TickCrossing(index);
            }

            ShowMist(BeastSightRules.Active(sight));
            _fogSight.Tick(_player.position, sight, Time.time);
            if (inCity && alive)
            {
                TickCollector();
            }
        }

        /// <summary>Counts the seconds of this fog spent in the map; at 20 s the fog is crossed and a drop is earned (once per fog).</summary>
        private void TickCrossing(long index)
        {
            if (index != _fogIndex)
            {
                _fogIndex = index;
                _fogSeconds = 0f;
                _fogEarned = false;
            }

            _fogSeconds += Time.deltaTime;
            if (_fogEarned || !GarajonayFogRules.Crossed(_fogSeconds))
            {
                return;
            }

            _fogEarned = true;
            if (!ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                return;
            }

            int day = ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;
            int earned = GarajonayFogRules.EarnedOn(progress.GetTimeMark(GarajonayFogRules.EarnedKey), day);
            int waiting = (int)Math.Max(0L, progress.GetTimeMark(GarajonayFogRules.WaitingKey));
            if (!GarajonayFogRules.CanEarn(earned, waiting))
            {
                return;
            }

            progress.SetTimeMark(GarajonayFogRules.EarnedKey, GarajonayFogRules.StoreEarned(day, earned + 1));
            progress.SetTimeMark(GarajonayFogRules.WaitingKey, waiting + 1);
            Say(RouteSightAirColdTexts.FogCrossed, RouteSightAirColdTexts.Get);
        }

        /// <summary>In the city the Coletor de Névoa hands over the waiting drops as herbs of the laurel forest.</summary>
        private void TickCollector()
        {
            if (Time.unscaledTime < _nextFogPay || !ServiceLocator.TryGet(out WorldMapProgress progress))
            {
                return;
            }

            _nextFogPay = Time.unscaledTime + FogPaySeconds;
            int waiting = (int)Math.Max(0L, progress.GetTimeMark(GarajonayFogRules.WaitingKey));
            if (waiting <= 0)
            {
                return;
            }

            _fogBag ??= _player.GetComponentInChildren<InventoryManager>();
            ItemDefinition herb = ItemDatabase.GetById(MealCatalog.Herb);
            if (_fogBag == null || herb == null)
            {
                return;
            }

            int leftover = _fogBag.AddItem(herb, waiting);
            progress.SetTimeMark(GarajonayFogRules.WaitingKey, leftover);
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (leftover >= waiting)
            {
                _nextFogPay = Time.unscaledTime + 30f;
                SayLine(RouteSightAirColdTexts.Get(localization, RouteSightAirColdTexts.FogCollectorFull));
                return;
            }

            SayLine(string.Format(RouteSightAirColdTexts.Get(localization, RouteSightAirColdTexts.FogCollector), herb.DisplayName, waiting - leftover));
        }

        /// <summary>Pale streaks of mist circling the hero at the edge of sight.</summary>
        private void ShowMist(bool show)
        {
            for (int i = 0; i < MistStreaks; i++)
            {
                LineRenderer streak = _mist[i];
                if (!show)
                {
                    if (streak != null)
                    {
                        streak.enabled = false;
                    }

                    continue;
                }

                if (streak == null)
                {
                    streak = _mist[i] = NewLine("Zf_GarajonayMist", 0.5f, 2);
                    streak.startColor = streak.endColor = MistColor;
                }

                streak.enabled = true;
                float a = (i / (float)MistStreaks + Time.time * 0.03f) * Mathf.PI * 2f;
                float r = GarajonayFogRules.SightRange + 0.5f + (i % 3) * 0.6f;
                float h = 0.4f + (i % 4) * 0.45f;
                Vector3 c = _player.position;
                Vector3 p0 = c + new Vector3(Mathf.Cos(a) * r, h, Mathf.Sin(a) * r);
                Vector3 p1 = c + new Vector3(Mathf.Cos(a + 0.5f) * r, h, Mathf.Sin(a + 0.5f) * r);
                streak.SetPosition(0, p0);
                streak.SetPosition(1, p1);
            }
        }

        private void LeaveFog()
        {
            if (!_inCanopy)
            {
                return;
            }

            _inCanopy = false;
            _fogPhase = GarajonayFogPhase.Clear;
            _fogSight.RevealAll();
            ShowMist(false);
        }

        private void Say(string key, Func<LocalizationManager, string, string> get)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayLine(get(localization, key));
        }

        private void SayLine(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, text);
            }
        }
    }
}
