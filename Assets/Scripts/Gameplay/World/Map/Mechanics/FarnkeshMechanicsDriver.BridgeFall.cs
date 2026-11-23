using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// 55 · falling off the Pontes de Vento (<see cref="WindBridgeFallRules"/>): the driver remembers the bridge the hero stepped on (and where),
    /// and a gust's push that takes them off its planks drops them into the Sisin's net, which throws them back on the bridge 12 m behind for a
    /// hit that never kills; once O Cortador de Cordas is freed (the save's registry of the defeated) he catches them where the gust hit. Only on
    /// the ground of the Pontes de Vento (the stone nests' bridges and the trails the lines cross never drop anyone). The cousin's line the first
    /// time the guard catches them is one mark in the save, set on that event.
    /// </summary>
    public sealed partial class FarnkeshMechanicsDriver
    {
        private int _bridgeLine = -1;
        private float _bridgeEntryAlong;
        private float _bridgeHitAlong;
        private float _bridgeBlownUntil = float.NegativeInfinity;
        private int _bridgeHitGust = -1;

        private void TickBridgeFall(Vector2 local)
        {
            Vector2 world = local + _origin;
            OverworldZone zone = OverworldRules.ZoneAt(_layout, world);
            bool onBridgeGround = zone != null && zone.Terrain == OverworldTerrain.WindBridges && OverworldRules.SettlementAt(_layout, world) == null
                && (_stats == null || _stats.IsAlive);
            int line = WindBridgeFallRules.Nearest(OverworldLayouts.WindBridgeLines, local, out float along, out float distance);
            if (!onBridgeGround || line < 0)
            {
                ForgetBridge();
                return;
            }

            if (_bridgeLine < 0)
            {
                if (WindBridgeFallRules.OnPlanks(distance))
                {
                    _bridgeLine = line;
                    _bridgeEntryAlong = along;
                }

                return;
            }

            // Measured against the bridge the hero is on (a push can bring another line nearer).
            NartesChainRules.Project(OverworldLayouts.WindBridgeLines[_bridgeLine], local, out along, out distance, out _);
            if (WindBridgeFallRules.OnPlanks(distance))
            {
                return;
            }

            if (WindBridgeFallRules.IsFall(distance, Time.time, _bridgeBlownUntil))
            {
                FallOffBridge();
            }
            else
            {
                // Walked off on their own, with no gust: no fall, the bridge is forgotten.
                ForgetBridge();
            }
        }

        /// <summary>Called by the gusts on each frame a push moves the hero: the moment the fall counts from, and where the gust first hit.</summary>
        private void NoteBridgePush(Vector2 local, int gust)
        {
            if (_bridgeLine < 0)
            {
                return;
            }

            if (_bridgeHitGust != gust)
            {
                _bridgeHitGust = gust;
                NartesChainRules.Project(OverworldLayouts.WindBridgeLines[_bridgeLine], local, out _bridgeHitAlong, out _, out _);
            }

            _bridgeBlownUntil = Time.time + WindBridgeFallRules.BlownGraceSeconds;
        }

        private void FallOffBridge()
        {
            Vector2[] line = OverworldLayouts.WindBridgeLines[_bridgeLine];
            bool guarded = ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(WindBridgeFallRules.GuardEnemyId);
            if (_stats != null)
            {
                float damage = WindBridgeFallRules.FallDamage(_stats.CurrentHealth, _stats.MaxHealth, guarded);
                if (damage > 0f)
                {
                    _stats.ApplyHazardDamage(damage);
                }
            }

            float landing = WindBridgeFallRules.LandingAlong(_bridgeEntryAlong, _bridgeHitAlong, guarded);
            Vector2 end = _origin + NartesChainRules.PointAt(line, landing);
            Vector2 ahead = _origin + NartesChainRules.PointAt(line, _bridgeHitAlong >= _bridgeEntryAlong ? landing + 2f : landing - 2f);
            Vector2 heading = ahead - end;
            _controller.ClickToMove?.ClearDestination();
            _view.WarmUp(end);
            CityModeController.PlaceAt(_player.gameObject, end, heading.sqrMagnitude > 0.01f ? heading : Vector2.up);
            _view.ResetTracking(end);

            // Still on the same bridge, from the same entry: a second gust pushes from where they landed.
            _bridgeBlownUntil = float.NegativeInfinity;
            SayRound2(guarded ? PeaksSteppeRound2Texts.BridgeCaught : PeaksSteppeRound2Texts.BridgeFall);
            if (guarded && ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(WindBridgeFallRules.FirstCatchKey) <= 0L)
            {
                progress.SetTimeMark(WindBridgeFallRules.FirstCatchKey, 1L);
                SayRound2(PeaksSteppeRound2Texts.LineFor(PeaksSteppeRound2Texts.BridgeLinePrefix, MapRequirementHeroState.Capture(null).Hero), 0.7f);
            }
        }

        private void ForgetBridge()
        {
            _bridgeLine = -1;
            _bridgeBlownUntil = float.NegativeInfinity;
            _bridgeHitGust = -1;
        }

        private void SayRound2(string key, float lift = 0f)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + lift), PeaksSteppeRound2Texts.Get(localization, key));
        }
    }
}
