using Rapadura.Core.DI;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The forgotten bridges of the gorges (32, <see cref="RootBridgeRules"/>): a withered bridge is a short brown line across its gorge; watered,
    /// it grows (the line reaches across in 10 s) and its root chest opens on the far side (a daily mark in the save). The growth belongs to
    /// this visit. No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float BridgeViewRange = 150f;
        private const float BridgeSpan = 16f;

        private static readonly Color WitheredColor = new Color(0.45f, 0.33f, 0.22f, 1f);
        private static readonly Color GrownColor = new Color(0.45f, 0.8f, 0.4f, 1f);
        private static readonly Color RootChestColor = new Color(0.95f, 0.85f, 0.4f, 1f);

        private LineRenderer[] _bridgeLines;
        private LineRenderer[] _rootChestLines;
        private float[] _bridgeGrowth;
        private float[] _bridgeGrownAt;
        private int _wateringBridge = -1;
        private int _bridgeHint = -1;
        private bool _inGorges;

        private void TickRootBridges(Vector2 local)
        {
            if (!OverworldLayouts.RootBridgeBounds.Contains(local))
            {
                LeaveRootBridges();
                return;
            }

            _inGorges = true;
            Vector2[] bridges = OverworldLayouts.RootBridges;
            if (_bridgeGrowth == null)
            {
                _bridgeGrowth = new float[bridges.Length];
                _bridgeGrownAt = new float[bridges.Length];
                for (int i = 0; i < bridges.Length; i++)
                {
                    _bridgeGrownAt[i] = float.NegativeInfinity;
                }
            }

            float now = Time.time;
            TickGrowth(local, bridges, now);
            ShowBridges(local, bridges, now);
            if (!_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            if (TickRootChest(local, now))
            {
                return;
            }

            int bridge = RootBridgeRules.Nearest(bridges, local, RootBridgeRules.WaterReach);
            if (bridge < 0 || RootBridgeRules.Grown(_bridgeGrownAt[bridge], now) || _wateringBridge == bridge)
            {
                if (bridge < 0)
                {
                    _bridgeHint = -1;
                }

                return;
            }

            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            bool canWater = RootBridgeRules.CanWater(hero, _bag != null && _bag.GetTotalCount(RootBridgeRules.WaterskinItemId) > 0);
            if (_bridgeHint != bridge || Time.unscaledTime >= _nextHint)
            {
                _bridgeHint = bridge;
                _nextHint = Time.unscaledTime + HintCooldown;
                SayR4(canWater ? CoastDeltaPassTexts.BridgePrompt : CoastDeltaPassTexts.BridgeNeedsWater);
            }

            if (!canWater)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_controller.Input.InteractPressedThisFrame)
            {
                _wateringBridge = bridge;
                _bridgeGrowth[bridge] = 0f;
                SayR4(CoastDeltaPassTexts.BridgeGrowing);
            }
        }

        private void TickGrowth(Vector2 local, Vector2[] bridges, float now)
        {
            if (_wateringBridge < 0)
            {
                return;
            }

            int b = _wateringBridge;
            bool near = _stats.IsAlive && (bridges[b] - local).sqrMagnitude <= RootBridgeRules.GrowReach * RootBridgeRules.GrowReach;
            _bridgeGrowth[b] = RootBridgeRules.Grow(_bridgeGrowth[b], near, Time.deltaTime);
            if (!near)
            {
                _wateringBridge = -1;
                return;
            }

            if (_bridgeGrowth[b] >= 1f)
            {
                _bridgeGrownAt[b] = now;
                _wateringBridge = -1;
                SayR4(CoastDeltaPassTexts.BridgeGrown);
            }
        }

        /// <summary>True while the hero stands at an open root chest.</summary>
        private bool TickRootChest(Vector2 local, float now)
        {
            int chest = RootBridgeRules.Nearest(OverworldLayouts.RootBridgeChests, local, RootBridgeRules.ChestReach);
            if (chest < 0)
            {
                return false;
            }

            ServiceLocator.TryGet(out WorldMapProgress progress);
            int day = GameDay();
            int opened = progress != null ? progress.GetDailyMark(RootBridgeRules.ChestKey(chest)) : 0;
            if (!RootBridgeRules.ChestOpens(RootBridgeRules.Grown(_bridgeGrownAt[chest], now), opened, day))
            {
                return false;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayR4(CoastDeltaPassTexts.BridgeChest);
            }

            if (_controller.Input.InteractPressedThisFrame && GiveR4(CostaTideRules.ChestItem(chest + 30, day)))
            {
                progress?.SetDailyMark(RootBridgeRules.ChestKey(chest), day);
            }

            return true;
        }

        private void ShowBridges(Vector2 local, Vector2[] bridges, float now)
        {
            _bridgeLines ??= new LineRenderer[bridges.Length];
            _rootChestLines ??= new LineRenderer[bridges.Length];
            for (int i = 0; i < bridges.Length; i++)
            {
                bool near = (bridges[i] - local).sqrMagnitude <= BridgeViewRange * BridgeViewRange;
                if (!near)
                {
                    Off(_bridgeLines[i]);
                    Off(_rootChestLines[i]);
                    continue;
                }

                bool grown = RootBridgeRules.Grown(_bridgeGrownAt[i], now);
                float reach = grown ? 1f : Mathf.Max(0.15f, _bridgeGrowth[i]);
                LineRenderer line = _bridgeLines[i] != null ? _bridgeLines[i] : (_bridgeLines[i] = NewLine("Dt_RootBridge", 1.2f));
                line.startColor = line.endColor = grown ? GrownColor : Color.Lerp(WitheredColor, GrownColor, _bridgeGrowth[i]);
                Vector2 a = _deltaOrigin + bridges[i] + new Vector2(0f, BridgeSpan * 0.5f);
                Vector2 b = _deltaOrigin + bridges[i] + new Vector2(0f, BridgeSpan * (0.5f - reach));
                line.SetPosition(0, new Vector3(a.x, Mathf.Max(0f, Ground(a)) + 0.4f, a.y));
                line.SetPosition(1, new Vector3(b.x, Mathf.Max(0f, Ground(b)) + 0.4f, b.y));
                line.enabled = true;

                if (grown)
                {
                    _rootChestLines[i] = Pillar(_rootChestLines[i], "Dt_RootChest", _deltaOrigin + OverworldLayouts.RootBridgeChests[i], 0.7f, 3f, RootChestColor);
                }
                else
                {
                    Off(_rootChestLines[i]);
                }
            }
        }

        private void LeaveRootBridges()
        {
            if (!_inGorges)
            {
                return;
            }

            _inGorges = false;
            _wateringBridge = -1;
            _bridgeHint = -1;
            Off(_bridgeLines);
            Off(_rootChestLines);
        }
    }
}
