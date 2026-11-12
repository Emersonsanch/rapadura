using Rapadura.Core.DI;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The sun shadows of Meroë (27, <see cref="SunShadowRules"/>): the three great pyramids drawn as lines, each shadow's tip as a dark ring
    /// that swings with the game clock, the chapel doors as two pylons. Standing on a tip while it lies on its door opens the chapel's chest
    /// (a daily mark in the save). No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float ShadowViewRange = 220f;
        private const int ShadowKeys = 600;

        private static readonly Color PyramidColor = new Color(0.72f, 0.45f, 0.32f, 1f);
        private static readonly Color ShadowTipColor = new Color(0.12f, 0.1f, 0.1f, 0.9f);
        private static readonly Color PylonColor = new Color(0.85f, 0.7f, 0.45f, 1f);

        private LineRenderer[] _pyramidLines;
        private LineRenderer[] _pylons;
        private float _shadowHeld;
        private int _shadowHintChapel = -1;
        private bool _inMeroe;

        private void TickShadows(Vector2 local)
        {
            if (!OverworldLayouts.MeroeShadowBounds.Contains(local))
            {
                LeaveShadows();
                return;
            }

            _inMeroe = true;
            float hour = GameHour();
            Vector2[] apexes = OverworldLayouts.MeroePyramids;
            ShowPyramids(local, apexes, hour);

            int chapel = SunShadowRules.NearestPyramid(apexes, local, SunShadowRules.DoorReach + 2f);
            if (chapel < 0 || !_stats.IsAlive)
            {
                _shadowHeld = 0f;
                _shadowHintChapel = -1;
                return;
            }

            if (chapel != _shadowHintChapel)
            {
                _shadowHintChapel = chapel;
                SayR4(CoastDeltaPassTexts.ShadowDoor);
            }

            bool onTip = SunShadowRules.OnTip(apexes[chapel], hour, local) && SunShadowRules.OnDoor(apexes[chapel], chapel, hour);
            if (!onTip)
            {
                _shadowHeld = 0f;
                return;
            }

            if (_shadowHeld == 0f)
            {
                SayR4(CoastDeltaPassTexts.ShadowHold);
            }

            _shadowHeld += Time.deltaTime;
            ServiceLocator.TryGet(out WorldMapProgress progress);
            int day = GameDay();
            int opened = progress != null ? progress.GetDailyMark(SunShadowRules.ChapelKey(chapel)) : 0;
            if (_shadowHeld < SunShadowRules.HoldSeconds)
            {
                return;
            }

            _shadowHeld = float.NegativeInfinity;
            if (!SunShadowRules.Opens(SunShadowRules.HoldSeconds, opened, day))
            {
                SayR4(CoastDeltaPassTexts.ShadowTaken);
                return;
            }

            SayR4(CoastDeltaPassTexts.ShadowOpened);
            if (GiveR4(CostaTideRules.ChestItem(chapel + 10, day)))
            {
                progress?.SetDailyMark(SunShadowRules.ChapelKey(chapel), day);
            }
        }

        private void ShowPyramids(Vector2 local, Vector2[] apexes, float hour)
        {
            if (_pyramidLines == null)
            {
                _pyramidLines = new LineRenderer[apexes.Length];
                _pylons = new LineRenderer[apexes.Length * 2];
            }

            bool shining = SunShadowRules.Shining(hour);
            for (int i = 0; i < apexes.Length; i++)
            {
                bool near = (apexes[i] - local).sqrMagnitude <= ShadowViewRange * ShadowViewRange;
                if (!near)
                {
                    Off(_pyramidLines[i]);
                    Off(_pylons[i * 2]);
                    Off(_pylons[i * 2 + 1]);
                    HideRing(ShadowKeys + i);
                    continue;
                }

                if (_pyramidLines[i] == null)
                {
                    _pyramidLines[i] = BuildPyramid(apexes[i]);
                }

                _pyramidLines[i].enabled = true;
                Vector2 door = SunShadowRules.ChapelDoor(apexes[i], i);
                Vector2 side = new Vector2(door.y - apexes[i].y, apexes[i].x - door.x).normalized * 2f;
                _pylons[i * 2] = Pillar(_pylons[i * 2], "Cs_ChapelPylon", _costaOrigin + door + side, 0.6f, 4f, PylonColor);
                _pylons[i * 2 + 1] = Pillar(_pylons[i * 2 + 1], "Cs_ChapelPylon", _costaOrigin + door - side, 0.6f, 4f, PylonColor);
                DrawRing(ShadowKeys + i, _costaOrigin, SunShadowRules.TipAt(apexes[i], hour), SunShadowRules.TipReach, ShadowTipColor, shining, false);
            }
        }

        /// <summary>The outline of a great pyramid: the base square and the four edges up to the apex, in one line.</summary>
        private LineRenderer BuildPyramid(Vector2 apex)
        {
            LineRenderer line = NewLine("Cs_GreatPyramid", 0.4f);
            line.startColor = line.endColor = PyramidColor;
            float h = OverworldLayouts.MeroePyramidHalfBase;
            Vector2 world = _costaOrigin + apex;
            float y = Mathf.Max(0f, Ground(world));
            Vector3 top = new Vector3(world.x, y + OverworldLayouts.MeroePyramidHeight, world.y);
            Vector3 a = new Vector3(world.x - h, y, world.y - h), b = new Vector3(world.x + h, y, world.y - h);
            Vector3 c = new Vector3(world.x + h, y, world.y + h), d = new Vector3(world.x - h, y, world.y + h);
            Vector3[] points = { a, b, c, d, a, top, b, c, top, d };
            line.positionCount = points.Length;
            line.SetPositions(points);
            return line;
        }

        private void LeaveShadows()
        {
            if (!_inMeroe)
            {
                return;
            }

            _inMeroe = false;
            _shadowHeld = 0f;
            _shadowHintChapel = -1;
            Off(_pyramidLines);
            Off(_pylons);
            for (int i = 0; i < OverworldLayouts.MeroePyramids.Length; i++)
            {
                HideRing(ShadowKeys + i);
            }
        }
    }
}
