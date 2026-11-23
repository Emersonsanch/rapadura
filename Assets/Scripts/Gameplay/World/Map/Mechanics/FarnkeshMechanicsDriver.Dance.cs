using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Baile do Vale (53, <see cref="SatyrDanceRules"/>), on the same driver as the gusts: at night, [E] on the stone stage of the Vale dos
    /// Sátiros starts a dance; a column of light on the stage flashes on every note of Marsyas's double flute and every [E] is a step, graded
    /// at once. The medal pays the first time in the UTC day (daily mark in the save, only when paid) and leaves a stamina recovery effect; the
    /// cousin's line on the first gold is kept in the save (one mark, on that event). Leaving the stage, mounting, a hit or the dawn ends it.
    /// </summary>
    public sealed partial class FarnkeshMechanicsDriver
    {
        private const string DanceBuffSource = "satyr_dance";
        private const string DanceGoldItemId = "item_gold";
        private const float DanceFlash = 0.12f;
        private const float DanceColumnHeight = 4f;

        private static readonly Color DanceIdleColor = new Color(0.95f, 0.8f, 0.35f, 0.35f);
        private static readonly Color DanceBeatColor = new Color(1f, 0.95f, 0.55f, 1f);

        private SatyrDanceRun _dance;
        private float _danceStartedAt;
        private bool _danceHit;
        private float _nextDanceHint;
        private LineRenderer _danceColumn;

        private void TickDance(Vector2 local)
        {
            Vector2 stage = OverworldLayouts.ValeSatirosOffset + SatyrDanceRules.StageFromTown;
            if (_dance != null)
            {
                TickDanceRun(local, stage);
                return;
            }

            ShowDanceColumn(false, stage, false);
            if (!SatyrDanceRules.OnStage(stage, local) || _controller.Input == null || (_stats != null && !_stats.IsAlive))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            bool night = IsDanceNight();
            if (Time.unscaledTime >= _nextDanceHint)
            {
                _nextDanceHint = Time.unscaledTime + HintCooldown * 2f;
                SayDance(night ? PeaksSteppeRoundTexts.DancePrompt : PeaksSteppeRoundTexts.DanceDay);
            }

            if (!night || !_controller.Input.InteractPressedThisFrame || IsDanceMounted())
            {
                return;
            }

            _dance = new SatyrDanceRun();
            _danceStartedAt = Time.time;
            _danceHit = false;
            SayDance(PeaksSteppeRoundTexts.DanceCountIn);
        }

        private void TickDanceRun(Vector2 local, Vector2 stage)
        {
            float t = Time.time - _danceStartedAt;
            if (_danceHit || SatyrDanceRules.LeftStage(stage, local) || IsDanceMounted() || !IsDanceNight() || (_stats != null && !_stats.IsAlive))
            {
                EndDance(false);
                return;
            }

            bool flash = false;
            for (int i = _dance.NextBeat; i < SatyrDanceRules.BeatCount; i++)
            {
                float off = t - SatyrDanceRules.BeatTime(i);
                if (off < -DanceFlash)
                {
                    break;
                }

                if (Mathf.Abs(off) <= DanceFlash)
                {
                    flash = true;
                    break;
                }
            }

            ShowDanceColumn(true, stage, flash);
            if (_controller.Input != null && _controller.Input.InteractPressedThisFrame)
            {
                SatyrStep step = _dance.Press(t);
                SayDance(step == SatyrStep.OnBeat ? PeaksSteppeRoundTexts.DanceOnBeat
                    : step == SatyrStep.Close ? PeaksSteppeRoundTexts.DanceClose : PeaksSteppeRoundTexts.DanceStumble);
            }
            else if (_dance.Tick(t) > 0)
            {
                SayDance(PeaksSteppeRoundTexts.DanceMissed);
            }

            if (_dance.Over(t))
            {
                EndDance(true);
            }
        }

        /// <summary>Ends the dance: on <paramref name="finished"/> the medal, its pay and its effect; otherwise only the flute stops.</summary>
        private void EndDance(bool finished)
        {
            if (_dance == null)
            {
                return;
            }

            SatyrDanceRun run = _dance;
            _dance = null;
            ShowDanceColumn(false, Vector2.zero, false);
            if (!finished)
            {
                if (_player != null)
                {
                    SayDance(PeaksSteppeRoundTexts.DanceStopped);
                }

                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int medal = SatyrDanceRules.Medal(run.Points);
            string medalKey = medal == 3 ? PeaksSteppeRoundTexts.DanceGold
                : medal == 2 ? PeaksSteppeRoundTexts.DanceSilver
                : medal == 1 ? PeaksSteppeRoundTexts.DanceBronze : PeaksSteppeRoundTexts.DanceNoMedal;
            SayGildText(PeaksSteppeRoundTexts.Format(localization, medalKey, run.Points, SatyrDanceRules.MaxPoints));
            if (medal <= 0)
            {
                return;
            }

            ApplyDanceBuff(SatyrDanceRules.BuffOf(medal));
            ServiceLocator.TryGet(out WorldMapProgress progress);
            int today = (int)NartesRaceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            bool first = progress != null && progress.GetDailyMark(SatyrDanceRules.PaidKey) != today;
            int purse = SatyrDanceRules.Purse(medal, first);
            if (purse > 0 && PayDance(purse))
            {
                progress.SetDailyMark(SatyrDanceRules.PaidKey, today);
                SayGildText(PeaksSteppeRoundTexts.Format(localization, PeaksSteppeRoundTexts.DancePaid, purse), 0.7f);
            }

            if (medal == 3 && progress != null && progress.GetTimeMark(SatyrDanceRules.FirstGoldKey) <= 0L)
            {
                progress.SetTimeMark(SatyrDanceRules.FirstGoldKey, 1L);
                SayDance(PeaksSteppeRoundTexts.LineFor(PeaksSteppeRoundTexts.DanceLinePrefix, MapRequirementHeroState.Capture(null).Hero), 1.4f);
            }
        }

        private bool PayDance(int amount)
        {
            ItemDefinition gold = ItemDatabase.GetById(DanceGoldItemId);
            return gold != null && _bag != null && _bag.CanFit(gold, amount) && _bag.AddItem(gold, amount) == 0;
        }

        private void ApplyDanceBuff(float value)
        {
            BuffController buffs = _player != null ? _player.GetComponent<BuffController>() : null;
            if (buffs == null || value <= 0f)
            {
                return;
            }

            buffs.RemoveEffectsBySource(DanceBuffSource);
            buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = "Passo do Baile",
                affectedStat = StatType.StaminaRegen,
                application = ModifierApplication.PercentAdditive,
                value = value,
                durationSeconds = SatyrDanceRules.BuffSeconds,
                isDebuff = false,
            }, DanceBuffSource);
        }

        private static bool IsDanceNight() => ServiceLocator.TryGet(out TimeOfDayManager clock) && clock.Phase == TimeOfDayPhase.Night;

        private static bool IsDanceMounted() => MountController.Active != null && MountController.Active.IsMounted;

        private void ShowDanceColumn(bool show, Vector2 stage, bool flash)
        {
            if (!show)
            {
                if (_danceColumn != null)
                {
                    _danceColumn.enabled = false;
                }

                return;
            }

            if (_danceColumn == null)
            {
                _danceColumn = NewLine("Fk_DanceColumn", 0.5f, DanceIdleColor);
            }

            Vector2 world = _origin + stage;
            float y = Mathf.Max(0f, OverworldRules.SurfaceAt(_layout, world));
            _danceColumn.SetPosition(0, new Vector3(world.x, y, world.y));
            _danceColumn.SetPosition(1, new Vector3(world.x, y + DanceColumnHeight * (flash ? 1.4f : 1f), world.y));
            _danceColumn.startWidth = _danceColumn.endWidth = flash ? 0.9f : 0.5f;
            _danceColumn.startColor = _danceColumn.endColor = flash ? DanceBeatColor : DanceIdleColor;
            _danceColumn.enabled = true;
        }

        private void SayDance(string key, float lift = 0f)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayGildText(PeaksSteppeRoundTexts.Get(localization, key), lift);
        }
    }
}
