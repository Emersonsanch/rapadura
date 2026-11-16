using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The shadow clock of the Torres Brancas (42, <see cref="ShadowClockRules"/>), on the same driver as the mirror clock: once the Little Eclipse
    /// is defeated (<see cref="DefeatedEnemyRegistry"/>) a dark gnomon stands on the south street with its shadow turning with the hour; [E] reads
    /// the hour and the north mirror's way. The cousin's line the first time is the only save mark (on that event).
    /// </summary>
    public sealed partial class MingchengMechanicsDriver
    {
        private const float ShadowClockViewRange = 80f;
        private const float ShadowLength = 3f;

        private static readonly Color GnomonColor = new Color(0.12f, 0.1f, 0.16f, 1f);
        private static readonly Color ShadowColor = new Color(0.05f, 0.05f, 0.08f, 0.85f);

        private LineRenderer _gnomon;
        private LineRenderer _shadow;
        private bool _eclipseFreed;
        private bool _eclipseAsked;

        private void TickShadowClock(Vector2 local, TimeOfDayManager clock)
        {
            Vector2 dial = ShadowClockRules.Dial;
            if ((dial - local).sqrMagnitude > ShadowClockViewRange * ShadowClockViewRange)
            {
                HideShadowClock();
                _eclipseAsked = false;
                return;
            }

            if (!_eclipseAsked)
            {
                _eclipseAsked = true;
                _eclipseFreed = ServiceLocator.TryGet(out DefeatedEnemyRegistry registry) && registry != null && registry.IsDefeated(ShadowClockRules.EclipseEnemyId);
            }

            if (!_eclipseFreed)
            {
                HideShadowClock();
                return;
            }

            float hour = clock != null ? clock.CurrentHour : 12f;
            ShowShadowClock(dial, hour);
            if (!ShadowClockRules.AtDial(dial, local) || _controller.Input == null)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayText(ShadowClockTexts.Get(localization, ShadowClockTexts.Prompt));
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            if (!ShadowClockRules.HasShadow(hour))
            {
                SayText(ShadowClockTexts.Get(localization, ShadowClockTexts.Night));
                return;
            }

            string face = ShadowClockTexts.Get(localization, ShadowClockTexts.FaceKey(ShadowClockRules.FirstMirrorState(hour)));
            SayText(string.Format(ShadowClockTexts.Get(localization, ShadowClockTexts.Reading), Mathf.FloorToInt(Mathf.Repeat(hour, 24f)),
                ShadowClockRules.HoursToNextBand(hour), face));
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(ShadowClockTexts.FirstReadKey) <= 0L)
            {
                progress.SetTimeMark(ShadowClockTexts.FirstReadKey, 1L);
                SayTextLifted(ShadowClockTexts.Get(localization, ShadowClockTexts.LineFor(MapRequirementHeroState.Capture(null).Hero)));
            }
        }

        private void ShowShadowClock(Vector2 dial, float hour)
        {
            Vector2 world = _origin + dial;
            float y = Ground(world);
            if (_gnomon == null)
            {
                _gnomon = NewLine("Mc_ShadowClockGnomon", 0.3f);
                _gnomon.startColor = _gnomon.endColor = GnomonColor;
                _gnomon.SetPosition(0, new Vector3(world.x, y, world.y));
                _gnomon.SetPosition(1, new Vector3(world.x, y + ShadowClockRules.GnomonHeight, world.y));
                _shadow = NewLine("Mc_ShadowClockShadow", 0.25f);
                _shadow.startColor = _shadow.endColor = ShadowColor;
            }

            _gnomon.enabled = true;
            Vector2 d = ShadowClockRules.ShadowDirection(hour);
            _shadow.enabled = d != Vector2.zero;
            if (_shadow.enabled)
            {
                _shadow.SetPosition(0, new Vector3(world.x, y + 0.05f, world.y));
                _shadow.SetPosition(1, new Vector3(world.x + d.x * ShadowLength, y + 0.05f, world.y + d.y * ShadowLength));
            }
        }

        private void HideShadowClock()
        {
            if (_gnomon != null)
            {
                _gnomon.enabled = false;
                _shadow.enabled = false;
            }
        }

        private void SayTextLifted(string text) =>
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), text);
    }
}
