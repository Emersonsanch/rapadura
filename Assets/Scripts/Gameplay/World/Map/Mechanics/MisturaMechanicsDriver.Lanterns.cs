using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The seven resin lanterns of the Cedral de Hobabish (80, <see cref="ResinLanternRules"/>) and the fire on O Rei do Piche's tar (79,
    /// <see cref="PitchKingBurnRules"/>), on the region's driver: a resin column on each lantern (gold when lit today), [E] or a fire skill lights it
    /// (daily mark, written on that event), and the count goes to <see cref="ResinLanternBoard"/> for Hobabish's mantles; every fire skill of the
    /// hero is recorded in <see cref="PitchKingBurnBoard"/> for the King's tar. The cousins' lines are kept in the save once (one mark each, on the event).
    /// </summary>
    public sealed partial class MisturaMechanicsDriver
    {
        private const float LanternViewRange = 60f;
        private const float LanternPostHeight = 3.2f;
        private const float LanternBoardRefresh = 0.5f;
        private const string ResinAllLitKey = "mistura.resin_lanterns.all_lit";
        private const string PitchBurntKey = "mistura.pitch_king.burnt";

        private static readonly Color ResinLit = new Color(1f, 0.72f, 0.25f, 1f);
        private static readonly Color ResinDark = new Color(0.35f, 0.28f, 0.18f, 0.8f);

        private readonly LineRenderer[] _resinPosts = new LineRenderer[ResinLanternRules.LanternCount];
        private readonly Vector2[] _resinSpots = new Vector2[ResinLanternRules.LanternCount];
        private readonly int[] _resinDays = new int[ResinLanternRules.LanternCount];
        private bool _resinPlaced;
        private float _nextResinRefresh;
        private float _nextResinHint;
        private int _pitchBurnsSeen = -1;

        private int ResinToday() => ServiceLocator.TryGet(out TimeOfDayManager clock) ? clock.Day : 1;

        private void PlaceResin()
        {
            if (_resinPlaced)
            {
                return;
            }

            _resinPlaced = true;
            for (int i = 0; i < _resinSpots.Length; i++)
            {
                _resinSpots[i] = OverworldLayouts.ResinLanternPosition(i);
            }
        }

        /// <summary>Reads the daily marks (twice a second) into the board, shows the lanterns near the hero and lights one on [E].</summary>
        private void TickResinLanterns(Vector2 local)
        {
            PlaceResin();
            SayPitchBurns();
            int today = ResinToday();
            if (Time.time >= _nextResinRefresh)
            {
                _nextResinRefresh = Time.time + LanternBoardRefresh;
                ServiceLocator.TryGet(out WorldMapProgress progress);
                for (int i = 0; i < _resinDays.Length; i++)
                {
                    _resinDays[i] = progress != null ? progress.GetDailyMark(ResinLanternRules.MarkKey(i)) : _resinDays[i];
                }

                ResinLanternBoard.Set(ResinLanternRules.LitCount(_resinDays, today));
            }

            for (int i = 0; i < _resinPosts.Length; i++)
            {
                bool show = (local - _resinSpots[i]).sqrMagnitude <= LanternViewRange * LanternViewRange;
                ShowResinPost(i, show, ResinLanternRules.LitToday(_resinDays[i], today));
            }

            int near = ResinLanternRules.LanternNear(_resinSpots, local, ResinLanternRules.Reach);
            if (near < 0 || _controller.Input == null)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            bool litAlready = ResinLanternRules.LitToday(_resinDays[near], today);
            if (Time.unscaledTime >= _nextResinHint)
            {
                _nextResinHint = Time.unscaledTime + HintCooldown;
                SayLink(litAlready ? LateBossLinkTexts.ResinAlready : LateBossLinkTexts.ResinPrompt);
            }

            if (_controller.Input.InteractPressedThisFrame)
            {
                _nextResinHint = Time.unscaledTime + HintCooldown;
                if (litAlready)
                {
                    SayLink(LateBossLinkTexts.ResinAlready);
                }
                else
                {
                    LightResin(near, today);
                }
            }
        }

        private void LightResin(int lantern, int today)
        {
            _resinDays[lantern] = today;
            bool known = ServiceLocator.TryGet(out WorldMapProgress progress);
            if (known)
            {
                progress.SetDailyMark(ResinLanternRules.MarkKey(lantern), today);
            }

            int lit = ResinLanternRules.LitCount(_resinDays, today);
            ResinLanternBoard.Set(lit);
            if (!ResinLanternRules.AllLit(lit))
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayText(string.Format(LateBossLinkTexts.Get(localization, LateBossLinkTexts.ResinLit), ResinLanternRules.MantleCap(lit)));
                return;
            }

            SayLink(LateBossLinkTexts.ResinAllLit);
            if (known && progress.GetTimeMark(ResinAllLitKey) <= 0L)
            {
                progress.SetTimeMark(ResinAllLitKey, 1L);
                SayLink(LateBossLinkTexts.LineFor(LateBossLinkTexts.Moment.ResinAllLit, MapRequirementHeroState.Capture(null).Hero));
            }
        }

        /// <summary>A fire skill of the hero: recorded for the King's tar, and it lights the resin lanterns it reaches (where it lands or where it was cast).</summary>
        private void OnFireForLinks(SkillCastEvent evt)
        {
            Vector2 landWorld = new Vector2(evt.TargetPoint.x, evt.TargetPoint.z);
            Vector2 castWorld = new Vector2(_player.position.x, _player.position.z);
            double now = Time.timeAsDouble;
            PitchKingBurnBoard.Record(landWorld, evt.Skill.AreaRadius, now);
            PitchKingBurnBoard.Record(castWorld, 0f, now);

            PlaceResin();
            int today = ResinToday();
            float fireReach = ResinLanternRules.FireReach + Mathf.Max(0f, evt.Skill.AreaRadius);
            int byLanding = ResinLanternRules.LanternNear(_resinSpots, landWorld - _origin, fireReach);
            int byCaster = ResinLanternRules.LanternNear(_resinSpots, castWorld - _origin, ResinLanternRules.FireReach);
            int lantern = byLanding >= 0 ? byLanding : byCaster;
            if (lantern >= 0 && !ResinLanternRules.LitToday(_resinDays[lantern], today))
            {
                LightResin(lantern, today);
            }
        }

        /// <summary>Speaks once per burn the board counted (the boss already warns its own); the first one ever adds the cousin's line.</summary>
        private void SayPitchBurns()
        {
            int burns = PitchKingBurnBoard.Burns;
            if (_pitchBurnsSeen < 0)
            {
                _pitchBurnsSeen = burns;
                return;
            }

            if (burns == _pitchBurnsSeen)
            {
                return;
            }

            _pitchBurnsSeen = burns;
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(PitchBurntKey) <= 0L)
            {
                progress.SetTimeMark(PitchBurntKey, 1L);
                SayLink(LateBossLinkTexts.LineFor(LateBossLinkTexts.Moment.PitchBurnt, MapRequirementHeroState.Capture(null).Hero));
            }
        }

        private void ShowResinPost(int index, bool show, bool lit)
        {
            LineRenderer post = _resinPosts[index];
            if (!show)
            {
                if (post != null)
                {
                    post.enabled = false;
                }

                return;
            }

            if (post == null)
            {
                post = _resinPosts[index] = NewLine("Mx_ResinLantern", 0.5f);
                Vector2 world = _origin + _resinSpots[index];
                float y = Ground(world);
                post.SetPosition(0, new Vector3(world.x, y + 1.2f, world.y));
                post.SetPosition(1, new Vector3(world.x, y + LanternPostHeight, world.y));
            }

            post.startColor = post.endColor = lit ? ResinLit : ResinDark;
            post.enabled = true;
        }

        private void LeaveResinLanterns()
        {
            for (int i = 0; i < _resinPosts.Length; i++)
            {
                if (_resinPosts[i] != null)
                {
                    _resinPosts[i].enabled = false;
                }
            }
        }

        private void SayLink(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(LateBossLinkTexts.Get(localization, key));
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
