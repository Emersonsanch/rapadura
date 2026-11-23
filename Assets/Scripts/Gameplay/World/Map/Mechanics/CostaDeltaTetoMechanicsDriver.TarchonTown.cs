using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// What the towns do with the reef's corals and the woken neighbours. Kirta (28, <see cref="KirtaMotherCoralRules"/>): the mother coral
    /// of the plaza wears a ring that goes from grey to coral pink with the corals of the reef lit (the recipe <c>CityTownRecipes.Kirta(int)</c>
    /// relights its branches), and Pughat or Kirta says what changed the first time the hero comes by after each new coral. Tarchon (33):
    /// old Tarchon's daily (<see cref="TarchonDailyRules"/>, a real <see cref="QuestType.Daily"/> quest started the first time the cousins walk
    /// in on each game day, one count per wake), the trols who come home as the total of woken neighbours grows (<see cref="TarchonCrowdRules"/>,
    /// NPC figures at <see cref="OverworldLayouts.TarchonCrowd"/>, each says hello once), and Larthia buying the freed wisps at her door
    /// (<see cref="LarthiaWispRules"/>). The saves are the marks of those rules, written only when something happens. Never takes the controls.
    /// </summary>
    public sealed partial class CostaDeltaTetoMechanicsDriver
    {
        private const int MotherCoralKey = 660;
        private const int LarthiaKey = 670;
        private const float MotherViewRange = 40f;

        private static readonly Color WispHeldColor = new Color(0.84f, 0.91f, 1f, 0.85f);

        private NpcFigure[] _crowdFigures;
        private LineRenderer _heldWispLine;
        private float _nextMotherCheck;
        private int _motherCoralsLit = -1;

        // ------------------------------------------------------------------
        // Kirta: the mother coral
        // ------------------------------------------------------------------

        private void TickMotherCoral(Vector2 local)
        {
            Vector2 mother = OverworldLayouts.KirtaMotherCoralAt;
            float distance = Vector2.Distance(local, mother);
            if (distance > MotherViewRange)
            {
                if (_motherCoralsLit >= 0)
                {
                    LeaveMotherCoral();
                }

                return;
            }

            WorldMapProgress progress = Progress();
            if (_motherCoralsLit < 0 || Time.unscaledTime >= _nextMotherCheck)
            {
                _nextMotherCheck = Time.unscaledTime + StateRefresh;
                _motherCoralsLit = CoralsLit(progress);
            }

            float glow = KirtaMotherCoralRules.Glow(_motherCoralsLit);
            Color color = Color.Lerp(CoralDimColor, IsNight() ? CoralNightColor : CoralLitColor, glow);
            DrawRing(MotherCoralKey, _costaOrigin, mother, TownKit.CoralMotherRadius + 0.6f, color, true, true);

            long seen = progress != null ? progress.GetTimeMark(KirtaMotherCoralRules.SeenKey) : 0L;
            if (progress != null && distance <= KirtaMotherCoralRules.HearRadius && KirtaMotherCoralRules.HasNews(_motherCoralsLit, seen))
            {
                progress.SetTimeMark(KirtaMotherCoralRules.SeenKey, _motherCoralsLit);
                SayTown(KirtaTarchonTownTexts.MotherLine(_motherCoralsLit));
            }
        }

        private void LeaveMotherCoral()
        {
            HideRing(MotherCoralKey);
            _motherCoralsLit = -1;
        }

        private static int CoralsLit(WorldMapProgress progress)
        {
            if (progress == null)
            {
                return 0;
            }

            int lit = 0;
            foreach (KirtaCoral coral in OverworldLayouts.KirtaCorals)
            {
                if (KirtaCoralRules.IsLit(progress.GetTimeMark(KirtaCoralRules.MarkKey(coral.Id))))
                {
                    lit++;
                }
            }

            return lit;
        }

        // ------------------------------------------------------------------
        // Tarchon: the daily, the crowd, Larthia
        // ------------------------------------------------------------------

        /// <summary>Every tick inside Tarchon's ground; <paramref name="busy"/> = a neighbour's prompt has the [E] this frame.</summary>
        private void TickTarchonTown(Vector2 local, WorldMapProgress progress, int today, int[] days, bool busy)
        {
            TickTarchonDaily(progress, today, days);
            ShowTarchonCrowd(progress);
            TickLarthia(local, progress, busy);
        }

        private static QuestManager Quests() => ServiceLocator.TryGet(out QuestManager quests) ? quests : null;

        private void TickTarchonDaily(WorldMapProgress progress, int today, int[] days)
        {
            QuestManager quests = Quests();
            if (quests == null || progress == null)
            {
                return;
            }

            int started = progress.GetDailyMark(TarchonDailyRules.StartedKey);
            if (TarchonDailyRules.Decide(started, today) != TarchonDailyStep.Begin)
            {
                return;
            }

            QuestDefinition daily = CityDailyQuestCatalog.GetQuest(TarchonDailyRules.QuestId);
            quests.RegisterDefinition(daily);
            quests.ResetDailyQuest(TarchonDailyRules.QuestId);
            progress.SetDailyMark(TarchonDailyRules.StartedKey, today);
            if (!quests.StartQuest(daily))
            {
                return;
            }

            int credit = TarchonDailyRules.Credit(TarchonLanternRules.AwakeCount(days, today));
            if (credit > 0)
            {
                quests.SetObjectiveProgress(TarchonDailyRules.QuestId, TarchonDailyRules.ObjectiveId, credit);
            }

            if (quests.IsActive(TarchonDailyRules.QuestId))
            {
                SayTown(KirtaTarchonTownTexts.DailyOffer, -1f);
            }
        }

        /// <summary>A neighbour just woke: the daily counts it, his wisp goes free and follows the hero.</summary>
        private void OnNeighbourWoken(WorldMapProgress progress)
        {
            QuestManager quests = Quests();
            if (quests != null && quests.IsActive(TarchonDailyRules.QuestId))
            {
                quests.UpdateObjectiveProgress(QuestObjectiveType.CustomFlag, TarchonDailyRules.ObjectiveTarget, 1);
                if (quests.IsCompleted(TarchonDailyRules.QuestId))
                {
                    SayTown(KirtaTarchonTownTexts.DailyDone, -2f);
                }
            }

            long held = progress != null ? progress.GetTimeMark(LarthiaWispRules.HeldKey) : 0L;
            ServiceLocator.TryGet(out LocalizationManager localization);
            if (LarthiaWispRules.Full(held))
            {
                SayTown(KirtaTarchonTownTexts.WispsFull, -2.5f);
                return;
            }

            int now = LarthiaWispRules.Catch(held);
            progress?.SetTimeMark(LarthiaWispRules.HeldKey, now);
            SayTown(KirtaTarchonTownTexts.Format(localization, KirtaTarchonTownTexts.WispFreed, now, LarthiaWispRules.MaxHeld), -2.5f, false);
        }

        private void ShowTarchonCrowd(WorldMapProgress progress)
        {
            TarchonNeighbour[] crowd = OverworldLayouts.TarchonCrowd;
            long total = progress != null ? progress.GetTimeMark(TarchonLanternRules.TotalKey) : 0L;
            int returned = Mathf.Min(crowd.Length, TarchonCrowdRules.Returned(total));
            _crowdFigures ??= new NpcFigure[crowd.Length];
            for (int i = 0; i < crowd.Length; i++)
            {
                if (i >= returned)
                {
                    if (_crowdFigures[i] != null)
                    {
                        Destroy(_crowdFigures[i].gameObject);
                        _crowdFigures[i] = null;
                    }

                    continue;
                }

                if (_crowdFigures[i] != null)
                {
                    continue;
                }

                Vector2 world = _deltaOrigin + crowd[i].Position;
                Vector2 toPlaza = OverworldLayouts.TarchonOffset - crowd[i].Position;
                float yaw = toPlaza.sqrMagnitude > 0.01f ? Mathf.Atan2(toPlaza.x, toPlaza.y) * Mathf.Rad2Deg : 180f;
                NpcFigure figure = NpcFigure.Create(transform, TarchonCrowdRecipes.For(crowd[i].Id), Vector3.zero);
                figure.transform.SetPositionAndRotation(new Vector3(world.x, Mathf.Max(0f, Ground(world)), world.y), Quaternion.Euler(0f, yaw, 0f));
                _crowdFigures[i] = figure;
            }

            long seen = progress != null ? progress.GetTimeMark(TarchonCrowdRules.SeenKey) : 0L;
            if (progress != null && TarchonCrowdRules.HasNews(returned, seen))
            {
                progress.SetTimeMark(TarchonCrowdRules.SeenKey, returned);
                ServiceLocator.TryGet(out LocalizationManager localization);
                SayTown(KirtaTarchonTownTexts.Format(localization, KirtaTarchonTownTexts.CrowdBack, returned, TarchonCrowdRules.MaxReturned), -0.5f, false);
                SayTown(KirtaTarchonTownTexts.CrowdLine(crowd[returned - 1].Id), -1f);
            }
        }

        private void TickLarthia(Vector2 local, WorldMapProgress progress, bool busy)
        {
            long held = progress != null ? progress.GetTimeMark(LarthiaWispRules.HeldKey) : 0L;
            ShowHeldWisps(held > 0);
            Vector2 trade = OverworldLayouts.TarchonLarthiaTrade;
            DrawRing(LarthiaKey, _deltaOrigin, trade, LarthiaWispRules.TradeReach, WispHeldColor, held > 0, true);
            if (busy || _controller.Input == null || !LarthiaWispRules.CanSell(held, Vector2.Distance(local, trade), _stats.IsAlive))
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            int gold = LarthiaWispRules.PriceFor(held);
            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (Time.unscaledTime >= _nextHint)
            {
                _nextHint = Time.unscaledTime + HintCooldown;
                SayTown(KirtaTarchonTownTexts.Format(localization, KirtaTarchonTownTexts.LarthiaPrompt, held, gold), 0f, false);
            }

            if (!_controller.Input.InteractPressedThisFrame || !PayGold(gold))
            {
                return;
            }

            _nextHint = Time.unscaledTime + HintCooldown;
            long sold = (progress != null ? progress.GetTimeMark(LarthiaWispRules.SoldKey) : 0L) + held;
            progress?.SetTimeMark(LarthiaWispRules.HeldKey, 0L);
            progress?.SetTimeMark(LarthiaWispRules.SoldKey, sold);
            SayTown(KirtaTarchonTownTexts.Format(localization, KirtaTarchonTownTexts.LarthiaSold, gold), 0f, false);
            SayTown(KirtaTarchonTownTexts.SellLineFor(MapRequirementHeroState.Capture(null).Hero), -0.5f);
            bool given = progress != null && progress.GetTimeMark(LarthiaWispRules.GiftKey) > 0L;
            if (progress != null && LarthiaWispRules.GiftDue(sold, given) && TryGive(TarchonLanternRules.WispLanternItem))
            {
                progress.SetTimeMark(LarthiaWispRules.GiftKey, 1L);
                SayTown(KirtaTarchonTownTexts.LarthiaGift, -1f);
            }

            ShowHeldWisps(false);
        }

        private void ShowHeldWisps(bool show)
        {
            if (!show || _player == null)
            {
                Off(_heldWispLine);
                return;
            }

            Vector3 side = _player.position - _player.right * 0.5f + Vector3.up * 0.9f;
            _heldWispLine = Pillar(_heldWispLine, "Dl_TarchonHeldWisps", new Vector2(side.x, side.z), 0.22f, 0.5f, WispHeldColor);
        }

        private void LeaveTarchonTown()
        {
            HideRing(LarthiaKey);
            Off(_heldWispLine);
            if (_crowdFigures == null)
            {
                return;
            }

            for (int i = 0; i < _crowdFigures.Length; i++)
            {
                if (_crowdFigures[i] != null)
                {
                    Destroy(_crowdFigures[i].gameObject);
                }

                _crowdFigures[i] = null;
            }
        }

        private void SayTown(string keyOrText, float drop = 0f, bool isKey = true)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            string text = isKey ? KirtaTarchonTownTexts.Get(localization, keyOrText) : keyOrText;
            Rapadura.Gameplay.Combat.CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + drop), text);
        }
    }

    /// <summary>
    /// The looks of the six trols who come home to Tarchon (<see cref="OverworldLayouts.TarchonCrowd"/>): the same tall, thin, moss-blue or
    /// bark-green folk as <see cref="TarchonNeighbourRecipes"/>, awake and busy, each with their own gesture.
    /// </summary>
    public static class TarchonCrowdRecipes
    {
        public static NpcRecipe For(string id)
        {
            NpcRecipe r = new NpcRecipe
            {
                Id = "tarchon_crowd_" + id,
                Region = BiomeType.Forest,
                Age = NpcAge.Adult,
                Build = NpcBuild.Slim,
                Height = 1.2f,
                Skin = NpcSwatch.SkinTrollMoss,
                Garment = NpcGarment.Tunic,
                Main = NpcSwatch.SwampLeaf,
                Sleeves = NpcSwatch.SwampLeaf,
                Trim = NpcSwatch.Rush,
                Trousers = NpcSwatch.WetWood,
                Boots = NpcSwatch.WetWood,
                Hair = NpcHair.Short,
                HairColor = NpcSwatch.SwampLeaf,
                Posture = NpcPosture.Relaxed,
                Face = NpcFace.Bright,
            };

            switch (id)
            {
                case "aule": // back from the reeds, still telling it
                    r.Hair = NpcHair.Long;
                    r.Beard = NpcBeard.Short;
                    r.Gesture = NpcGesture.Storytell;
                    break;
                case "seianti": // the weaver, at her spindle again
                    r.Skin = NpcSwatch.SkinTrollBark;
                    r.Hair = NpcHair.Bun;
                    r.HairColor = NpcSwatch.HairDark;
                    r.Garment = NpcGarment.Dress;
                    r.Main = NpcSwatch.LilacMist;
                    r.Sleeves = NpcSwatch.LilacMist;
                    r.Gesture = NpcGesture.Spin;
                    break;
                case "thana": // came to see who woke her brother
                    r.Hair = NpcHair.TwoBraids;
                    r.Garment = NpcGarment.Kaftan;
                    r.Layers = NpcLayer.Scarf;
                    r.LayerColor = NpcSwatch.NetTwine;
                    r.Gesture = NpcGesture.Chatter;
                    break;
                case "sethre": // listens to the planks
                    r.Skin = NpcSwatch.SkinTrollBark;
                    r.Garment = NpcGarment.Robe;
                    r.Height = 1.26f;
                    r.Gesture = NpcGesture.Scan;
                    break;
                case "hasti": // keeps the east boardwalk with a lantern
                    r.Hair = NpcHair.Spiky;
                    r.Layers = NpcLayer.Belt | NpcLayer.Satchel;
                    r.LayerColor = NpcSwatch.WetWood;
                    r.RightArm = NpcArmPose.HoldStaff;
                    r.Props = new[] { NpcProp.LanternPole };
                    r.Gesture = NpcGesture.LanternRaise;
                    break;
                default: // tite, the grandson
                    r.Age = NpcAge.Youth;
                    r.Height = 1.1f;
                    r.Gesture = NpcGesture.Call;
                    break;
            }

            return r;
        }
    }
}
