using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The mirror clock of the Torres Brancas (42, <see cref="MirrorClockRules"/>), on the same driver as the wall and the reflection: four
    /// mirrors (a post with a bar showing where each faces) round the white city, [E] turns one a quarter, and the light (bright lines from the
    /// spire from mirror to mirror) goes on as far as they are right for the hour. All four right opens the spire's vault: a chest of the
    /// reflection's table, once per game day (a daily mark in the save). The mirrors' turns belong to this visit; never takes the controls.
    /// </summary>
    public sealed partial class MingchengMechanicsDriver
    {
        private const float MirrorClockViewRange = 220f;
        private const float MirrorHeight = 3f;
        private const float BeamHeight = 6f;

        private static readonly Color MirrorPostColor = new Color(0.92f, 0.92f, 0.88f, 1f);
        private static readonly Color MirrorFaceColor = new Color(0.75f, 0.9f, 1f, 1f);
        private static readonly Color BeamColor = new Color(1f, 0.97f, 0.75f, 0.95f);

        private readonly int[] _mirrorStates = { 0, 0, 0, 0 };
        private LineRenderer[] _mirrorPosts;
        private LineRenderer[] _mirrorFaces;
        private LineRenderer[] _beamLines;
        private int _litBefore = -1;
        private bool _inTorres;

        private void TickMirrorClock(Vector2 local, TimeOfDayManager clock)
        {
            if (!OverworldLayouts.MirrorClockBounds.Contains(local))
            {
                LeaveMirrorClock();
                return;
            }

            _inTorres = true;
            float hour = clock != null ? clock.CurrentHour : 12f;
            int day = clock != null ? clock.Day : 1;
            Vector2[] mirrors = OverworldLayouts.MirrorClockMirrors;
            int lit = MirrorClockRules.LitChain(_mirrorStates, hour);
            ShowMirrorClock(local, mirrors, lit);

            int mirror = MirrorClockRules.MirrorAt(mirrors, local);
            if (mirror < 0 || _controller.Input == null)
            {
                return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayMirror(DreamsHerdGroveMirrorsTexts.MirrorPrompt);
            }

            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            _mirrorStates[mirror] = MirrorClockRules.Turn(_mirrorStates[mirror]);
            _nextHint = Time.unscaledTime + HintCooldown;
            lit = MirrorClockRules.LitChain(_mirrorStates, hour);
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (lit < MirrorClockRules.MirrorCount)
            {
                SayText(string.Format(DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.MirrorLit), lit));
                return;
            }

            ServiceLocator.TryGet(out WorldMapProgress progress);
            int taken = progress != null ? progress.GetDailyMark(MirrorClockRules.VaultKey) : 0;
            if (!MirrorClockRules.VaultOpens(true, taken, day))
            {
                SayMirror(DreamsHerdGroveMirrorsTexts.MirrorTaken);
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(MingchengReflectionRules.ChestItem(0, day));
            if (item == null)
            {
                return;
            }

            if (_bag == null || _bag.AddItem(item, 1) != 0)
            {
                SayMirror(DreamsHerdGroveMirrorsTexts.BagFull);
                return;
            }

            progress?.SetDailyMark(MirrorClockRules.VaultKey, day);
            SayText(string.Format(DreamsHerdGroveMirrorsTexts.Get(localization, DreamsHerdGroveMirrorsTexts.MirrorVault), item.DisplayName));
        }

        private void ShowMirrorClock(Vector2 local, Vector2[] mirrors, int lit)
        {
            if (_mirrorPosts == null)
            {
                _mirrorPosts = new LineRenderer[mirrors.Length];
                _mirrorFaces = new LineRenderer[mirrors.Length];
                _beamLines = new LineRenderer[mirrors.Length];
            }

            bool near = (OverworldLayouts.TorresBrancasOffset - local).sqrMagnitude <= MirrorClockViewRange * MirrorClockViewRange;
            Vector2 from = OverworldLayouts.TorresBrancasOffset;
            for (int i = 0; i < mirrors.Length; i++)
            {
                if (!near)
                {
                    SetEnabled(_mirrorPosts[i], false);
                    SetEnabled(_mirrorFaces[i], false);
                    SetEnabled(_beamLines[i], false);
                    continue;
                }

                Vector3 foot = new Vector3(_origin.x + mirrors[i].x, Ground(_origin + mirrors[i]), _origin.y + mirrors[i].y);
                if (_mirrorPosts[i] == null)
                {
                    _mirrorPosts[i] = NewLine("Mc_MirrorPost", 0.3f);
                    _mirrorPosts[i].startColor = _mirrorPosts[i].endColor = MirrorPostColor;
                    _mirrorPosts[i].SetPosition(0, foot);
                    _mirrorPosts[i].SetPosition(1, foot + Vector3.up * MirrorHeight);
                    _mirrorFaces[i] = NewLine("Mc_MirrorFace", 0.5f);
                    _mirrorFaces[i].startColor = _mirrorFaces[i].endColor = MirrorFaceColor;
                    _beamLines[i] = NewLine("Mc_MirrorBeam", 0.25f);
                    _beamLines[i].startColor = _beamLines[i].endColor = BeamColor;
                }

                Vector2 face = MirrorClockRules.Facing(_mirrorStates[i]);
                Vector3 top = foot + Vector3.up * MirrorHeight;
                _mirrorFaces[i].SetPosition(0, top);
                _mirrorFaces[i].SetPosition(1, top + new Vector3(face.x, 0f, face.y) * 1.5f);
                _mirrorPosts[i].enabled = true;
                _mirrorFaces[i].enabled = true;

                // The beam reaches mirror i when every mirror before it is right (the spire lights the first).
                bool beam = i <= lit;
                if (beam)
                {
                    Vector2 a = i == 0 ? from : mirrors[i - 1];
                    _beamLines[i].SetPosition(0, new Vector3(_origin.x + a.x, Ground(_origin + a) + BeamHeight, _origin.y + a.y));
                    _beamLines[i].SetPosition(1, foot + Vector3.up * MirrorHeight);
                }

                _beamLines[i].enabled = beam;
            }

            if (lit != _litBefore)
            {
                _litBefore = lit;
            }
        }

        private static void SetEnabled(LineRenderer line, bool on)
        {
            if (line != null)
            {
                line.enabled = on;
            }
        }

        private void LeaveMirrorClock()
        {
            if (!_inTorres)
            {
                return;
            }

            _inTorres = false;
            _litBefore = -1;
            if (_mirrorPosts == null)
            {
                return;
            }

            for (int i = 0; i < _mirrorPosts.Length; i++)
            {
                SetEnabled(_mirrorPosts[i], false);
                SetEnabled(_mirrorFaces[i], false);
                SetEnabled(_beamLines[i], false);
            }
        }

        private void SayMirror(string key)
        {
            ServiceLocator.TryGet(out LocalizationManager localization);
            SayText(DreamsHerdGroveMirrorsTexts.Get(localization, key));
        }
    }
}
