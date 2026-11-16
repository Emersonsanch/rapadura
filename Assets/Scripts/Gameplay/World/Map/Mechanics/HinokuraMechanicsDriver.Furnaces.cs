using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.EventRooms;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The temperature of the nine furnaces of the Pátio das Fornalhas (69, <see cref="NineFurnacesPuzzle"/>), on the same driver as the forges:
    /// each furnace's bellows is a beacon on the ring whose height and colour show its heat (blue-grey cold, orange at the balance of 5, white-hot
    /// at 9); [E] within 5 m pumps it. The ring lives in memory (a server would hold the same nine numbers for everyone in the court) and settles
    /// into a new fire after 3 minutes without a pump or after a balance; the save keeps only the UTC day the court's forge paid and the moment
    /// it first did.
    /// </summary>
    public sealed partial class HinokuraMechanicsDriver
    {
        private const float FurnaceViewRange = 160f;
        private const float FurnaceRuleRange = 50f;
        private const float FurnaceBaseHeight = 2f;
        private const float FurnaceHeightPerHeat = 1.6f;

        private static readonly Color FurnaceCold = new Color(0.45f, 0.55f, 0.7f, 0.9f);
        private static readonly Color FurnaceRight = new Color(1f, 0.55f, 0.15f, 1f);
        private static readonly Color FurnaceWhite = new Color(1f, 0.97f, 0.85f, 1f);

        private int[] _furnaces;
        private long _furnaceDay = long.MinValue;
        private bool _furnaceNight;
        private int _furnaceRound;
        private float _lastPump = float.NegativeInfinity;
        private LineRenderer[] _furnaceBeacons;
        private Vector2[] _bellows;
        private long _furnaceRuleDay = long.MinValue;
        private int _hintFurnace = -1;
        private float _nextFurnaceHint;

        private static bool FurnaceNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private void TickFurnaces(Vector2 local)
        {
            if ((local - OverworldLayouts.NineFurnacesCenter).sqrMagnitude >= FurnaceViewRange * FurnaceViewRange)
            {
                HideFurnaces();
                return;
            }

            _bellows ??= BuildBellows();
            long day = UtcDay();
            bool night = FurnaceNight();
            bool idle = Time.time - _lastPump >= NineFurnacesPuzzle.IdleResetSeconds;
            if (_furnaces == null || day != _furnaceDay || (idle && night != _furnaceNight))
            {
                // A new day (or the night falling on an idle ring) lights a new fire; a ring someone is working on keeps its fire.
                _furnaceDay = day;
                _furnaceNight = night;
                _furnaceRound = 0;
                SettleFurnaces(false);
            }
            else if (idle && _lastPump > float.NegativeInfinity)
            {
                _furnaceRound++;
                SettleFurnaces(true);
            }

            ShowFurnaces();
            if (!_stats.IsAlive)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            if (_furnaceRuleDay != day && (local - OverworldLayouts.NineFurnacesCenter).sqrMagnitude < FurnaceRuleRange * FurnaceRuleRange)
            {
                _furnaceRuleDay = day;
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.FurnaceRule));
            }

            int furnace = NineFurnacesPuzzle.FurnaceAt(_bellows, local);
            if (furnace < 0)
            {
                _hintFurnace = -1;
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (_hintFurnace != furnace || Time.unscaledTime >= _nextFurnaceHint)
            {
                _hintFurnace = furnace;
                _nextFurnaceHint = Time.unscaledTime + HintCooldown * 2f;
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnaceGauge, furnace + 1, _furnaces[furnace],
                    _furnaces[NineFurnacesPuzzle.Left(furnace)], _furnaces[NineFurnacesPuzzle.Right(furnace)], NineFurnacesPuzzle.BalancedCount(_furnaces)));
            }

            if (PuzzleInteract())
            {
                PumpFurnace(furnace, localization);
            }
        }

        private static Vector2[] BuildBellows()
        {
            var bellows = new Vector2[NineFurnacesPuzzle.Count];
            for (int i = 0; i < bellows.Length; i++)
            {
                bellows[i] = OverworldLayouts.NineFurnace(i);
            }

            return bellows;
        }

        private void SettleFurnaces(bool say)
        {
            _furnaces = NineFurnacesPuzzle.Start(NineFurnacesPuzzle.SeedFor(_furnaceDay, _furnaceRound, _furnaceNight), NineFurnacesPuzzle.ScramblesFor(_furnaceNight));
            _lastPump = float.NegativeInfinity;
            if (say)
            {
                SayPuzzle(HinokuraPuzzleTexts.FurnaceSettled);
            }
        }

        private void PumpFurnace(int furnace, LocalizationManager localization)
        {
            _nextFurnaceHint = Time.unscaledTime + HintCooldown * 2f;
            FurnacePumpResult result = NineFurnacesPuzzle.Pump(_furnaces, furnace);
            switch (result)
            {
                case FurnacePumpResult.TooHot:
                    SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnaceTooHot, furnace + 1));
                    return;
                case FurnacePumpResult.NeighbourTooCold:
                    SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnaceNeighbourCold, furnace + 1));
                    return;
                case FurnacePumpResult.Balanced:
                    // Balanced already: a reward left due by a full bag is paid now (nothing happens once it was paid).
                    PayFurnaces(localization);
                    return;
            }

            _lastPump = Time.time;
            if (!NineFurnacesPuzzle.IsBalanced(_furnaces))
            {
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnacePumped, furnace + 1, _furnaces[furnace], NineFurnacesPuzzle.BalancedCount(_furnaces)));
                return;
            }

            SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.FurnaceBalanced));
            string line = HinokuraPuzzleTexts.FurnaceLineFor(PuzzleHero());
            if (line != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), HinokuraPuzzleTexts.Get(localization, line));
            }

            PayFurnaces(localization);
        }

        /// <summary>The court's forge pays once a UTC day; a full bag leaves it due: [E] on a bellows of the still balanced ring (3 minutes) pays it.</summary>
        private void PayFurnaces(LocalizationManager localization)
        {
            ServiceLocator.TryGet(out WorldMapProgress progress);
            long day = UtcDay();
            bool paid = progress != null && progress.GetDailyMark(NineFurnacesPuzzle.DayKey) == (int)day;
            bool first = progress == null || progress.GetTimeMark(NineFurnacesPuzzle.FirstKey) <= 0L;
            (int gold, int stones, int mixture) = NineFurnacesPuzzle.Reward(paid, first);
            if (gold <= 0)
            {
                return;
            }

            if (!MechanicsBagGrant.Grant(PuzzleBag(), (MechanicsBagGrant.GoldItemId, gold), (EventRoomRules.BlessingStoneItemId, stones), (RefineItems.ChaosStone, mixture)))
            {
                SayText(HinokuraPuzzleTexts.Get(localization, HinokuraPuzzleTexts.BagFull));
                return;
            }

            progress?.SetDailyMark(NineFurnacesPuzzle.DayKey, (int)day);
            if (first)
            {
                progress?.SetTimeMark(NineFurnacesPuzzle.FirstKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnaceRewardFirst, gold, stones, mixture));
            }
            else
            {
                SayText(HinokuraPuzzleTexts.Format(localization, HinokuraPuzzleTexts.FurnaceReward, gold, stones));
            }
        }

        private void ShowFurnaces()
        {
            if (_furnaceBeacons == null)
            {
                _furnaceBeacons = new LineRenderer[NineFurnacesPuzzle.Count];
                for (int i = 0; i < _furnaceBeacons.Length; i++)
                {
                    _furnaceBeacons[i] = NewLine("Hinokura_FurnaceBellows", 0.55f);
                    _furnaceBeacons[i].SetPosition(0, Ground(_bellows[i]));
                }
            }

            for (int i = 0; i < _furnaceBeacons.Length; i++)
            {
                LineRenderer beacon = _furnaceBeacons[i];
                int heat = _furnaces[i];
                beacon.enabled = true;
                beacon.SetPosition(1, beacon.GetPosition(0) + Vector3.up * (FurnaceBaseHeight + heat * FurnaceHeightPerHeat));
                Color color = heat == NineFurnacesPuzzle.Balance ? FurnaceRight
                    : heat < NineFurnacesPuzzle.Balance ? Color.Lerp(FurnaceCold, FurnaceRight, (heat - NineFurnacesPuzzle.MinHeat) / (float)(NineFurnacesPuzzle.Balance - NineFurnacesPuzzle.MinHeat) * 0.6f)
                    : Color.Lerp(FurnaceRight, FurnaceWhite, (heat - NineFurnacesPuzzle.Balance) / (float)(NineFurnacesPuzzle.MaxHeat - NineFurnacesPuzzle.Balance));
                beacon.startColor = beacon.endColor = color;
            }
        }

        private void HideFurnaces()
        {
            for (int i = 0; _furnaceBeacons != null && i < _furnaceBeacons.Length; i++)
            {
                _furnaceBeacons[i].enabled = false;
            }

            _hintFurnace = -1;
        }

        private void SayPuzzle(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(HinokuraPuzzleTexts.Get(localization, key));
        }
    }
}
