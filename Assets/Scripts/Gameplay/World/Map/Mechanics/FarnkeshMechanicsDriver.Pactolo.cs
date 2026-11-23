using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Pactolo's water on O Ouro de Midas (53, <see cref="MidasWashRules"/>), on the same driver as the gilding: [E] standing in the river
    /// fills the hands (three pours, kept ten minutes while in the region); [E] by the statue pours one: a stun on it and, in
    /// <see cref="MidasWashBoard"/>, 6 s in which its golden touch does nothing (read by the boss's ability). The pours belong to this visit
    /// (never in the save); the cousin's line the first time is kept in the save (one mark, on that event).
    /// </summary>
    public sealed partial class FarnkeshMechanicsDriver
    {
        private const float WashHintCooldown = 4f;

        private int _washPours;
        private double _washFilledAt;
        private bool _washInRiver;
        private float _nextWashHint;
        private int _washStoppedSeen;

        private void TickMidasWash(Vector2 local)
        {
            double now = Time.timeAsDouble;
            int before = _washPours;
            _washPours = MidasWashRules.PoursLeft(_washPours, _washFilledAt, now);
            if (before > 0 && _washPours == 0)
            {
                SayWash(PeaksSteppeRoundTexts.WashDried);
            }

            SayWashStopped();
            if (_stats != null && !_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            bool inRiver = PactoloGildRules.InRiver(OverworldLayouts.PactoloRiver, local);
            bool enteredRiver = inRiver && !_washInRiver;
            _washInRiver = inRiver;
            if (inRiver)
            {
                if (!MidasWashRules.CanFill(true, _washPours))
                {
                    return;
                }

                InteractPromptRegistry.Ping(Time.unscaledTime);
                if (enteredRiver || Time.unscaledTime >= _nextWashHint)
                {
                    _nextWashHint = Time.unscaledTime + WashHintCooldown * 3f;
                    SayWash(PeaksSteppeRoundTexts.WashFillPrompt);
                }

                if (_controller.Input.InteractPressedThisFrame)
                {
                    _washPours = MidasWashRules.Pours;
                    _washFilledAt = now;
                    ServiceLocator.TryGet(out LocalizationManager localization);
                    SayGildText(PeaksSteppeRoundTexts.Format(localization, PeaksSteppeRoundTexts.WashFilled, _washPours, MidasWashRules.Pours));
                }

                return;
            }

            if (_washPours <= 0)
            {
                return;
            }

            EnemyController statue = FindMidasStatue();
            float distance = statue != null ? FlatDistance(statue.transform.position, _player.position) : float.MaxValue;
            if (!MidasWashRules.CanPour(_washPours, statue != null, distance))
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextWashHint)
            {
                _nextWashHint = Time.unscaledTime + WashHintCooldown;
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayGildText(PeaksSteppeRoundTexts.Format(localization, PeaksSteppeRoundTexts.WashPourPrompt, _washPours));
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _washPours--;
            _nextWashHint = Time.unscaledTime + WashHintCooldown;
            MidasWashBoard.Poured(now);
            statue.Buffs?.ApplyStun(MidasWashRules.StunSeconds);
            SayWash(PeaksSteppeRoundTexts.WashPoured);
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(MidasWashRules.FirstPourKey) <= 0L)
            {
                progress.SetTimeMark(MidasWashRules.FirstPourKey, 1L);
                SayWash(PeaksSteppeRoundTexts.LineFor(PeaksSteppeRoundTexts.WashLinePrefix, MapRequirementHeroState.Capture(null).Hero), 0.7f);
            }
        }

        /// <summary>Speaks once per touch the water stopped (the boss reads the board, the driver tells the hero why nothing happened).</summary>
        private void SayWashStopped()
        {
            if (MidasWashBoard.Stopped == _washStoppedSeen)
            {
                return;
            }

            _washStoppedSeen = MidasWashBoard.Stopped;
            SayWash(PeaksSteppeRoundTexts.WashTouchStopped);
        }

        /// <summary>Out of the region (or the driver stops): the water runs out of the hands.</summary>
        private void ClearMidasWash()
        {
            _washPours = 0;
            _washInRiver = false;
            _washStoppedSeen = MidasWashBoard.Stopped;
        }

        private static EnemyController FindMidasStatue()
        {
            foreach (EnemyController enemy in EnemyController.EnabledEnemies)
            {
                if (enemy != null && enemy.Definition != null && enemy.Definition.name == PactoloGildRules.MidasGoldEnemyId && enemy.Health != null && enemy.Health.IsAlive)
                {
                    return enemy;
                }
            }

            return null;
        }

        private static float FlatDistance(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

        private void SayWash(string key, float lift = 0f)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayGildText(PeaksSteppeRoundTexts.Get(localization, key), lift);
        }
    }
}
