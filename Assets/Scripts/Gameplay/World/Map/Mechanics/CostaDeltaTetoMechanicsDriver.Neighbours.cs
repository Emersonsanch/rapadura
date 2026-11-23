using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Despertar o vizinho and the swamp night of the Palafitas de Tarchon (33, <see cref="TarchonLanternRules"/>). Five dimmed trols (Arnth,
    /// Velia, Laris, Ravnthu, Fasti) stand still on Tarchon's plaza and streets as NPC figures, stooped and staring; [E] at the Mastro dos
    /// Fogos-Fátuos puts a lit lantern in the hero's hands (a wisp-white stroke beside them, 120 s), and [E] beside a dimmed neighbour hands it
    /// over: he wakes for the game day (a daily mark, written only then), stands up with the lantern raised and says his line; the cousin says
    /// one on the day's first wake and Thanchvil pays her purse once a day for the five. At night, inside the town's ground
    /// (<see cref="OverworldLayouts.TarchonNightBounds"/>), the lilac mist hides the beasts past 18 m unless a lantern is lit or the
    /// Lanterna de Fogo-Fátuo is in the bag (<see cref="BeastSightHider"/>). No damage, never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const float NeighbourViewRange = 70f;
        private const int MastKey = 600;
        private const int NeighbourKeys = 601;

        private static readonly Color NeighbourDimColor = new Color(0.55f, 0.6f, 0.66f, 0.8f);
        private static readonly Color LanternColor = new Color(0.84f, 0.91f, 1f, 1f);

        private readonly BeastSightHider _tarchonSight = new BeastSightHider();
        private NpcFigure[] _neighbourFigures;
        private bool[] _neighbourFigureAwake;
        private LineRenderer _lanternLine;
        private float _lanternLitAt = float.NegativeInfinity;
        private bool _lanternWasLit;
        private bool _inNeighbours;
        private bool _mistSaid;
        private bool _wispLanternInBag;
        private bool[] _neighbourAwake;
        private int[] _neighbourDays;
        private float _nextWispBagCheck;

        private void TickNeighbours(Vector2 local)
        {
            bool inNight = OverworldLayouts.TarchonNightBounds.Contains(local);
            bool lantern = TarchonLanternRules.LanternLit(_lanternLitAt, Time.time);
            // The lights the freed Pescador de Luzes let go (FisherWisps partial) clear the night mist like a lantern, for 10 min.
            TickTarchonNight(inNight, lantern || FisherWispsClearNight());

            TarchonNeighbour[] neighbours = OverworldLayouts.TarchonNeighbours;
            if (!inNight || (local - OverworldLayouts.TarchonOffset).sqrMagnitude > NeighbourViewRange * NeighbourViewRange)
            {
                LeaveNeighbours();
                return;
            }

            _inNeighbours = true;
            WorldMapProgress progress = Progress();
            int today = GameDay();
            bool[] awake = _neighbourAwake ??= new bool[neighbours.Length];
            int[] days = _neighbourDays ??= new int[neighbours.Length];
            for (int i = 0; i < neighbours.Length; i++)
            {
                days[i] = progress != null ? progress.GetDailyMark(TarchonLanternRules.MarkKey(neighbours[i].Id)) : 0;
                awake[i] = TarchonLanternRules.AwakeToday(days[i], today);
            }

            ShowNeighbours(neighbours, awake);
            ShowLantern(lantern);
            if (_lanternWasLit && !lantern)
            {
                SayNeighbour(TarchonLanternTexts.LanternOut);
            }

            _lanternWasLit = lantern;
            if (!_stats.IsAlive || _controller.Input == null)
            {
                return;
            }

            Vector2 mast = OverworldLayouts.TarchonOffset + CityTownRecipes.TarchonWispMast;
            bool anyDim = TarchonLanternRules.AwakeCount(days, today) < TarchonLanternRules.NeighbourCount;
            DrawRing(MastKey, _deltaOrigin, mast, TarchonLanternRules.MastReach, LanternColor, anyDim && !lantern, true);

            int near = TarchonLanternRules.Nearest(neighbours, local, TarchonLanternRules.NeighbourReach);
            TickTarchonTown(local, progress, today, days, near >= 0);
            if (near >= 0)
            {
                TickNeighbour(near, neighbours[near], awake[near], lantern, local, progress, days, today);
                return;
            }

            if (!lantern && anyDim && TarchonLanternRules.CanLightAtMast(Vector2.Distance(local, mast), _stats.IsAlive))
            {
                InteractPromptRegistry.Ping(Time.unscaledTime);
                HintNeighbour(TarchonLanternTexts.MastPrompt);
                if (_controller.Input.InteractPressedThisFrame)
                {
                    _lanternLitAt = Time.time;
                    _lanternWasLit = true;
                    _nextHint = Time.unscaledTime + HintCooldown;
                    SayNeighbour(TarchonLanternTexts.LanternLit);
                }
            }
        }

        private void TickNeighbour(int index, TarchonNeighbour neighbour, bool awake, bool lantern, Vector2 local, WorldMapProgress progress, int[] days, int today)
        {
            NeighbourCheck check = TarchonLanternRules.Check(awake, lantern, Vector2.Distance(local, neighbour.Position));
            switch (check)
            {
                case NeighbourCheck.AlreadyAwake:
                    HintNeighbour(TarchonLanternTexts.AlreadyAwake);
                    return;
                case NeighbourCheck.NoLantern:
                    HintNeighbour(TarchonLanternTexts.NoLantern);
                    return;
                case NeighbourCheck.TooFar:
                    return;
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            HintNeighbour(TarchonLanternTexts.NeighbourPrompt);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            bool firstToday = TarchonLanternRules.AwakeCount(days, today) == 0;
            days[index] = today;
            progress?.SetDailyMark(TarchonLanternRules.MarkKey(neighbour.Id), today);
            progress?.SetTimeMark(TarchonLanternRules.TotalKey, progress.GetTimeMark(TarchonLanternRules.TotalKey) + 1L);
            _lanternLitAt = float.NegativeInfinity;
            _lanternWasLit = false;
            _nextHint = Time.unscaledTime + HintCooldown;

            ServiceLocator.TryGet(out LocalizationManager localization);
            int count = TarchonLanternRules.AwakeCount(days, today);
            SayText(string.Format(TarchonLanternTexts.Get(localization, TarchonLanternTexts.Woke), count, TarchonLanternRules.NeighbourCount));
            SayNeighbour(TarchonLanternTexts.NeighbourLine(neighbour.Id), -0.5f);
            OnNeighbourWoken(progress);
            if (firstToday)
            {
                SayNeighbour(TarchonLanternTexts.LineFor(MapRequirementHeroState.Capture(null).Hero), -1f);
            }

            int purseDay = progress != null ? progress.GetDailyMark(TarchonLanternRules.PurseKey) : 0;
            if (TarchonLanternRules.PaysPurse(count, purseDay, today))
            {
                SayNeighbour(TarchonLanternTexts.AllAwake, -1.5f);
                if (PayGold(TarchonLanternRules.AllAwakePurse))
                {
                    progress?.SetDailyMark(TarchonLanternRules.PurseKey, today);
                }
            }
        }

        private void TickTarchonNight(bool inNight, bool lantern)
        {
            if (Time.unscaledTime >= _nextWispBagCheck)
            {
                _nextWispBagCheck = Time.unscaledTime + StateRefresh;
                _wispLanternInBag = _bag != null && _bag.GetTotalCount(TarchonLanternRules.WispLanternItem) > 0;
            }

            float range = TarchonLanternRules.SightRange(IsNight(), inNight, lantern, _wispLanternInBag);
            _tarchonSight.Tick(_player.position, range, Time.time);
            if (!BeastSightRules.Active(range))
            {
                if ((lantern || _wispLanternInBag) && _mistSaid && inNight && IsNight())
                {
                    _mistSaid = false;
                    SayNeighbour(TarchonLanternTexts.LanternSight);
                }

                if (!inNight || !IsNight())
                {
                    _mistSaid = false;
                }

                return;
            }

            if (!_mistSaid)
            {
                _mistSaid = true;
                SayNeighbour(TarchonLanternTexts.NightMist);
            }
        }

        private void ShowNeighbours(TarchonNeighbour[] neighbours, bool[] awake)
        {
            _neighbourFigures ??= new NpcFigure[neighbours.Length];
            _neighbourFigureAwake ??= new bool[neighbours.Length];
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (_neighbourFigures[i] == null || _neighbourFigureAwake[i] != awake[i])
                {
                    if (_neighbourFigures[i] != null)
                    {
                        Destroy(_neighbourFigures[i].gameObject);
                    }

                    Vector2 world = _deltaOrigin + neighbours[i].Position;
                    Vector2 toPlaza = OverworldLayouts.TarchonOffset - neighbours[i].Position;
                    float yaw = toPlaza.sqrMagnitude > 0.01f ? Mathf.Atan2(toPlaza.x, toPlaza.y) * Mathf.Rad2Deg : 180f;
                    NpcFigure figure = NpcFigure.Create(transform, TarchonNeighbourRecipes.For(neighbours[i].Id, awake[i]), Vector3.zero);
                    figure.transform.SetPositionAndRotation(new Vector3(world.x, Mathf.Max(0f, Ground(world)), world.y), Quaternion.Euler(0f, yaw, 0f));
                    _neighbourFigures[i] = figure;
                    _neighbourFigureAwake[i] = awake[i];
                }

                DrawRing(NeighbourKeys + i, _deltaOrigin, neighbours[i].Position, TarchonLanternRules.NeighbourReach, NeighbourDimColor, !awake[i], true);
            }
        }

        private void ShowLantern(bool lit)
        {
            if (!lit)
            {
                Off(_lanternLine);
                return;
            }

            Vector3 side = _player.position + _player.right * 0.45f;
            _lanternLine = Pillar(_lanternLine, "Dl_TarchonLantern", new Vector2(side.x, side.z), 0.35f, 2f, LanternColor);
        }

        private void LeaveNeighbours()
        {
            if (!_inNeighbours)
            {
                return;
            }

            _inNeighbours = false;
            HideRing(MastKey);
            for (int i = 0; i < TarchonLanternRules.NeighbourCount; i++)
            {
                HideRing(NeighbourKeys + i);
            }

            Off(_lanternLine);
            LeaveTarchonTown();
            if (_neighbourFigures != null)
            {
                for (int i = 0; i < _neighbourFigures.Length; i++)
                {
                    if (_neighbourFigures[i] != null)
                    {
                        Destroy(_neighbourFigures[i].gameObject);
                    }

                    _neighbourFigures[i] = null;
                }
            }
        }

        /// <summary>Off the Delta: the beasts show again, the neighbours and the lantern go (the lantern is held only in the town).</summary>
        private void LeaveTarchon()
        {
            _tarchonSight.RevealAll();
            _mistSaid = false;
            _lanternLitAt = float.NegativeInfinity;
            _lanternWasLit = false;
            LeaveNeighbours();
        }

        private void HintNeighbour(string key)
        {
            if (Time.unscaledTime < _nextHint)
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            SayNeighbour(key);
        }

        private void SayNeighbour(string key, float drop = 0f)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), TarchonLanternTexts.Get(localization, key));
        }
    }

    /// <summary>
    /// The looks of the five dimmed trols of Tarchon (<see cref="OverworldLayouts.TarchonNeighbours"/>), kept here and not in
    /// <see cref="NpcRecipes"/> since they are figures of the mechanic, not residents of the town recipe: tall and thin, moss-blue or bark-green,
    /// in the town's wet wood, rush and net twine. Dimmed: stooped, arms hanging, dreamy face, faded clothes. Awake: upright, bright face, the
    /// lantern pole raised and the leaf or lilac of the town back on.
    /// </summary>
    public static class TarchonNeighbourRecipes
    {
        public static NpcRecipe For(string id, bool awake)
        {
            NpcRecipe r = new NpcRecipe
            {
                Id = "tarchon_neighbour_" + id + (awake ? "_awake" : "_dim"),
                Region = BiomeType.Forest,
                Age = NpcAge.Adult,
                Build = NpcBuild.Slim,
                Height = 1.22f,
                Skin = NpcSwatch.SkinTrollMoss,
                Garment = NpcGarment.Tunic,
                Trim = NpcSwatch.Rush,
                Trousers = NpcSwatch.WetWood,
                Boots = NpcSwatch.WetWood,
                HairColor = NpcSwatch.SwampLeaf,
            };

            switch (id)
            {
                case "arnth": // the net fisher: spiky moss hair, a belt and satchel
                    r.Hair = NpcHair.Spiky;
                    r.Layers = NpcLayer.Belt | NpcLayer.Satchel;
                    r.LayerColor = NpcSwatch.WetWood;
                    break;
                case "velia": // the weaver: bark skin, a bun, a dress and an apron of twine
                    r.Skin = NpcSwatch.SkinTrollBark;
                    r.Hair = NpcHair.Bun;
                    r.HairColor = NpcSwatch.HairDark;
                    r.Garment = NpcGarment.Dress;
                    r.Layers = NpcLayer.Apron;
                    r.LayerColor = NpcSwatch.NetTwine;
                    break;
                case "laris": // the reed boatman: long hair and a short beard, a robe
                    r.Hair = NpcHair.Long;
                    r.Beard = NpcBeard.Short;
                    r.Garment = NpcGarment.Robe;
                    r.Height = 1.26f;
                    break;
                case "ravnthu": // the singer of the stilts: two braids, a kaftan and a scarf
                    r.Skin = NpcSwatch.SkinTrollBark;
                    r.Hair = NpcHair.TwoBraids;
                    r.Garment = NpcGarment.Kaftan;
                    r.Layers = NpcLayer.Scarf;
                    r.LayerColor = NpcSwatch.LilacMist;
                    break;
                default: // fasti, the youngest: short hair, a plain tunic
                    r.Age = NpcAge.Youth;
                    r.Hair = NpcHair.Short;
                    r.Height = 1.12f;
                    break;
            }

            if (awake)
            {
                r.Posture = NpcPosture.Relaxed;
                r.Face = NpcFace.Bright;
                r.Main = id == "velia" || id == "ravnthu" ? NpcSwatch.LilacMist : NpcSwatch.SwampLeaf;
                r.Sleeves = r.Main;
                r.LeftArm = NpcArmPose.Hang;
                r.RightArm = NpcArmPose.HoldStaff;
                r.Props = new[] { NpcProp.LanternPole };
                r.Gesture = NpcGesture.LanternRaise;
            }
            else
            {
                r.Posture = NpcPosture.Stooped;
                r.Face = NpcFace.Dreamy;
                r.Main = NpcSwatch.NetTwine;
                r.Sleeves = NpcSwatch.NetTwine;
                r.Trim = NpcSwatch.WetWood;
                r.LeftArm = NpcArmPose.Hang;
                r.RightArm = NpcArmPose.Hang;
                r.Props = new NpcProp[0];
                r.Gesture = NpcGesture.None;
            }

            return r;
        }
    }
}
