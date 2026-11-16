using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The lanterns and the drum of Nian's ring as the boss reads them (45, <see cref="NianScareRules"/>): the scene's state of this fight, in world
    /// coordinates, written by <see cref="MingchengMechanicsDriver"/> and read by the boss's pounce (<c>RegionBossAbilities</c>, NoiseFright).
    /// Online the server keeps this state; times are <c>Time.timeAsDouble</c> on both sides.
    /// </summary>
    public static class NianScareBoard
    {
        private static readonly Vector2[] s_lanterns = new Vector2[NianScareRules.LanternCount];
        private static readonly double[] s_litUntil = new double[NianScareRules.LanternCount];
        private static readonly double[] s_coolUntil = new double[NianScareRules.LanternCount];
        private static double s_lastBeat = double.NegativeInfinity;

        /// <summary>How many times a lantern or the drum frightened Nian (the driver speaks when it changes).</summary>
        public static int Frights { get; private set; }

        /// <summary>Whether the last fright was the drum (else a lantern).</summary>
        public static bool LastByDrum { get; private set; }

        public static IReadOnlyList<double> LitUntil => s_litUntil;
        public static IReadOnlyList<double> CoolUntil => s_coolUntil;
        public static double LastBeat => s_lastBeat;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            for (int i = 0; i < NianScareRules.LanternCount; i++)
            {
                s_lanterns[i] = Vector2.zero;
                s_litUntil[i] = 0d;
                s_coolUntil[i] = 0d;
            }

            s_lastBeat = double.NegativeInfinity;
            Frights = 0;
            LastByDrum = false;
        }

        /// <summary>Puts the lanterns round the ring at <paramref name="arenaWorld"/>.</summary>
        public static void Place(Vector2 arenaWorld)
        {
            for (int i = 0; i < NianScareRules.LanternCount; i++)
            {
                s_lanterns[i] = NianScareRules.LanternAt(arenaWorld, i);
            }
        }

        public static bool Light(int lantern, double now)
        {
            if (lantern < 0 || lantern >= NianScareRules.LanternCount || !NianScareRules.CanLight(s_litUntil[lantern], s_coolUntil[lantern], now))
            {
                return false;
            }

            s_litUntil[lantern] = now + NianScareRules.LitSeconds;
            return true;
        }

        public static bool Beat(double now)
        {
            if (!NianScareRules.CanDrum(s_lastBeat, now))
            {
                return false;
            }

            s_lastBeat = now;
            return true;
        }

        /// <summary>
        /// Whether the drum or a red lantern frightens Nian's pounce on <paramref name="markWorld"/>, warned at <paramref name="warningAt"/>; the
        /// lantern that does goes out and cools.
        /// </summary>
        public static bool Scares(Vector2 markWorld, double warningAt, double now)
        {
            if (NianScareRules.DrumScares(s_lastBeat, warningAt, now))
            {
                Frights++;
                LastByDrum = true;
                return true;
            }

            int lantern = NianScareRules.ShelteringLantern(s_lanterns, s_litUntil, now, markWorld);
            if (lantern < 0)
            {
                return false;
            }

            s_litUntil[lantern] = 0d;
            s_coolUntil[lantern] = now + NianScareRules.CoolSeconds;
            Frights++;
            LastByDrum = false;
            return true;
        }
    }

    /// <summary>
    /// The red lanterns and the drum round Nian's ring (45, <see cref="NianScareRules"/>), on the same driver as the wall: three lantern posts
    /// (red when lit) and a drum by the stone ring of the Portão do Nian; [E] lights a lantern or beats the drum, and the boss reads them in
    /// <see cref="NianScareBoard"/>. The cousin's line the first time they frighten it is kept in the save (one mark, on that event).
    /// </summary>
    public sealed partial class MingchengMechanicsDriver
    {
        private const float NianRange = 60f;
        private const float NianPostHeight = 2.6f;
        private const string NianScaredKey = "mingcheng_nian_scared";

        private static readonly Color NianLanternLit = new Color(1f, 0.15f, 0.1f, 1f);
        private static readonly Color NianLanternDark = new Color(0.35f, 0.12f, 0.1f, 1f);
        private static readonly Color NianDrumColor = new Color(0.55f, 0.3f, 0.15f, 1f);

        private readonly LineRenderer[] _nianLanterns = new LineRenderer[NianScareRules.LanternCount];
        private LineRenderer _nianDrum;
        private bool _nearNian;
        private bool _nianPlaced;
        private int _nianFrightsSeen;

        private void TickNian(Vector2 local)
        {
            Vector2 arena = OverworldLayouts.NianArena;
            if ((local - arena).sqrMagnitude > NianRange * NianRange)
            {
                LeaveNian();
                return;
            }

            if (!_nianPlaced)
            {
                _nianPlaced = true;
                NianScareBoard.Place(_origin + arena);
                _nianFrightsSeen = NianScareBoard.Frights;
            }

            _nearNian = true;
            double now = Time.timeAsDouble;
            ShowNian(arena, now);
            SayNianFrights();
            if (_controller.Input == null)
            {
                return;
            }

            int lantern = NianScareRules.LanternNear(arena, local);
            bool drum = lantern < 0 && NianScareRules.AtDrum(arena, local);
            if (lantern < 0 && !drum)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayNian(drum ? NianScareTexts.DrumPrompt : NianScareTexts.LanternPrompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            if (drum)
            {
                SayNian(NianScareBoard.Beat(now) ? NianScareTexts.DrumBeat : NianScareTexts.DrumResting);
                return;
            }

            if (NianScareBoard.Light(lantern, now))
            {
                SayNian(NianScareTexts.LanternLit);
            }
            else if (NianScareRules.IsLit(NianScareBoard.LitUntil[lantern], now))
            {
                SayNian(NianScareTexts.LanternBurning);
            }
            else
            {
                ServiceLocator.TryGet(out LocalizationManager localization);
                int wait = Mathf.Max(1, Mathf.CeilToInt((float)(NianScareBoard.CoolUntil[lantern] - now)));
                SayText(string.Format(NianScareTexts.Get(localization, NianScareTexts.LanternCooling), wait));
            }
        }

        /// <summary>Speaks once per fright the board counted (the boss already shouts its own); the first one ever adds the cousin's line.</summary>
        private void SayNianFrights()
        {
            int frights = NianScareBoard.Frights;
            if (frights == _nianFrightsSeen)
            {
                return;
            }

            _nianFrightsSeen = frights;
            SayNian(NianScareBoard.LastByDrum ? NianScareTexts.FrightDrum : NianScareTexts.LanternSwatted);
            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress.GetTimeMark(NianScaredKey) <= 0L)
            {
                progress.SetTimeMark(NianScaredKey, 1L);
                SayNian(NianScareTexts.LineFor(MapRequirementHeroState.Capture(null).Hero));
            }
        }

        private void ShowNian(Vector2 arena, double now)
        {
            for (int i = 0; i < _nianLanterns.Length; i++)
            {
                if (_nianLanterns[i] == null)
                {
                    _nianLanterns[i] = NewLine("Mc_NianLantern", 0.45f);
                    Vector2 world = _origin + NianScareRules.LanternAt(arena, i);
                    float y = Ground(world);
                    _nianLanterns[i].SetPosition(0, new Vector3(world.x, y, world.y));
                    _nianLanterns[i].SetPosition(1, new Vector3(world.x, y + NianPostHeight, world.y));
                }

                Color color = NianScareRules.IsLit(NianScareBoard.LitUntil[i], now) ? NianLanternLit : NianLanternDark;
                _nianLanterns[i].startColor = _nianLanterns[i].endColor = color;
                _nianLanterns[i].enabled = true;
            }

            if (_nianDrum == null)
            {
                _nianDrum = NewLine("Mc_NianDrum", 1.1f);
                _nianDrum.startColor = _nianDrum.endColor = NianDrumColor;
                Vector2 world = _origin + NianScareRules.DrumAt(arena);
                float y = Ground(world);
                _nianDrum.SetPosition(0, new Vector3(world.x, y, world.y));
                _nianDrum.SetPosition(1, new Vector3(world.x, y + 0.9f, world.y));
            }

            _nianDrum.enabled = true;
        }

        private void LeaveNian()
        {
            if (!_nearNian)
            {
                return;
            }

            _nearNian = false;
            for (int i = 0; i < _nianLanterns.Length; i++)
            {
                if (_nianLanterns[i] != null)
                {
                    _nianLanterns[i].enabled = false;
                }
            }

            if (_nianDrum != null)
            {
                _nianDrum.enabled = false;
            }
        }

        private void SayNian(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(NianScareTexts.Get(localization, key));
        }
    }
}
