using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Scene side of the two services of the Cais de Mesektet that need a screen (<see cref="MesektetServiceRules"/>): Djedi's circles of
    /// light (East street) and Setne's shelf of the Caderno (South street). Each service point stands right in front of the keeper's door
    /// (<see cref="DoorOf"/>, read from the town layout, so it follows the recipe); [E] there opens a <see cref="CityServicePanel"/>. Same
    /// shape as <c>CityServicesMechanicsDriver</c> (added to the overworld root, off where the layout has no Rio das Doze Horas). Saves: the
    /// daily mark of the free lesson and the count of shelves received. The lesson is one 15-minute effect on the <see cref="BuffController"/>
    /// (a new circle replaces the old one).
    /// </summary>
    public sealed class MesektetServicesMechanicsDriver : MonoBehaviour, IRegionMechanics
    {
        private const string PlayerTag = "Player";
        public const string LessonSource = "city_djedi_lesson";
        private const float HintCooldown = 6f;
        private const float FeedbackHeight = 2.4f;
        private const float AreaHalf = 40f;

        private Transform _player;
        private PlayerController _controller;
        private PlayerStats _stats;
        private InventoryManager _bag;
        private BuffController _buffs;
        private Vector2 _lessonPoint;
        private Vector2 _shelfPoint;
        private Rect[] _areas;
        private int _hintPoint = -1;
        private float _nextHint;

        private float _lessonUntil = float.NegativeInfinity;
        private DjediLesson _lessonLit;

        /// <inheritdoc/>
        public Rect[] MechanicsAreas => _areas;

        /// <inheritdoc/>
        public bool MechanicsBusy => false;

        /// <summary>Where the cousins stand to talk to <paramref name="residentId"/> in the Cais de Mesektet (town-local, x = east, z = north).</summary>
        public static Vector2 DoorOf(string residentId)
        {
            CityLayout town = CityTownRecipes.LayoutFor(CityTownRecipes.MesektetCityId);
            CityLot lot = town?.LotOfResident(residentId);
            return lot != null ? lot.DoorApproach : Vector2.zero;
        }

        private void Start()
        {
            OverworldView view = GetComponent<OverworldView>();
            OverworldZone anchor = view != null && view.Layout != null ? view.Layout.Zone(OverworldLayouts.RioAnchorZoneId) : null;
            if (anchor == null)
            {
                enabled = false;
                return;
            }

            Vector2 city = anchor.Origin + OverworldLayouts.CaisMesektetOffset;
            _lessonPoint = city + DoorOf("djedi");
            _shelfPoint = city + DoorOf("setne");
            _areas = new[] { new Rect(city - new Vector2(AreaHalf, AreaHalf), new Vector2(AreaHalf * 2f, AreaHalf * 2f)) };
        }

        private void OnDisable() => _hintPoint = -1;

        private void Update()
        {
            if (_areas == null || !FindPlayer())
            {
                return;
            }

            TickLesson();
            bool alive = _stats == null || _stats.IsAlive;
            ServiceLocator.TryGet(out DialogueManager dialogue);
            if (!alive || _controller.Input == null || CityServicePanel.IsOpen || (dialogue != null && dialogue.IsActive))
            {
                _hintPoint = -1;
                return;
            }

            Vector2 hero = new Vector2(_player.position.x, _player.position.z);
            int at = CityServiceRules.InReach(_lessonPoint, hero, MesektetServiceRules.LessonReach) ? 0
                : CityServiceRules.InReach(_shelfPoint, hero, MesektetServiceRules.ShelfReach) ? 1 : -1;
            if (at < 0)
            {
                _hintPoint = -1;
                return;
            }

            if (_hintPoint != at || Time.unscaledTime >= _nextHint)
            {
                _hintPoint = at;
                _nextHint = Time.unscaledTime + HintCooldown;
                Say(at == 0 ? CityTrainingTexts.LessonPrompt : CityTrainingTexts.ShelfPrompt);
            }

            InteractPromptRegistry.Ping(Time.unscaledTime);
            if (!_controller.Input.InteractPressedThisFrame)
            {
                return;
            }

            CityServicePanel.Open(at == 0 ? (ICityServiceScreen)new LessonScreen(this) : new ShelfScreen(this));
        }

        private bool FindPlayer()
        {
            if (_player == null)
            {
                GameObject go = GameObject.FindGameObjectWithTag(PlayerTag);
                if (go == null)
                {
                    return false;
                }

                _player = go.transform;
                _controller = go.GetComponent<PlayerController>();
                _stats = go.GetComponent<PlayerStats>();
                _bag = go.GetComponentInChildren<InventoryManager>();
                _buffs = go.GetComponent<BuffController>();
            }

            return _controller != null;
        }

        private static bool InCombat()
        {
            MountController mount = MountController.Active;
            return mount != null && MountRules.IsInCombat(mount.SecondsSinceCombat);
        }

        private static CharacterId? Hero() => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        private static long Today() => CityServiceRules.DayOf(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        private static WorldMapProgress Progress() => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress : null;

        private int Gold => _bag != null ? _bag.GetTotalCount(RefineItems.Gold) : 0;

        // ------------------------------------------------------------------
        // Djedi's circles of light
        // ------------------------------------------------------------------

        private float LessonLeft => _buffs != null && _buffs.HasEffectsFromSource(LessonSource) ? _lessonUntil - Time.time : 0f;

        private void TickLesson()
        {
            if (_lessonUntil > float.NegativeInfinity && Time.time >= _lessonUntil)
            {
                _lessonUntil = float.NegativeInfinity;
                Say(CityTrainingTexts.LessonEnd);
            }
        }

        /// <summary>Lights <paramref name="lesson"/>: the status line for the screen.</summary>
        private string Learn(DjediLesson lesson, LocalizationManager localization)
        {
            long today = Today();
            WorldMapProgress progress = Progress();
            long freeDay = progress != null ? progress.GetDailyMark(MesektetServiceRules.LessonDailyKey) : 0;
            int cost = MesektetServiceRules.LessonCost(freeDay, today);
            MesektetServiceCheck check = MesektetServiceRules.CheckLesson(InCombat(), cost, Gold);
            if (check == MesektetServiceCheck.InCombat)
            {
                return CityTrainingTexts.Get(localization, CityTrainingTexts.LessonCombat);
            }

            if (check == MesektetServiceCheck.NotEnoughGold || _buffs == null || (cost > 0 && !_bag.RemoveById(RefineItems.Gold, cost)))
            {
                return CityTrainingTexts.Format(localization, CityTrainingTexts.LessonGoldMissing, cost);
            }

            if (cost == 0)
            {
                progress?.SetDailyMark(MesektetServiceRules.LessonDailyKey, (int)today);
            }

            _buffs.RemoveEffectsBySource(LessonSource);
            _buffs.ApplyEffect(new StatModifierDefinition
            {
                displayName = CityTrainingTexts.Get(localization, CityTrainingTexts.LessonName(lesson)),
                affectedStat = MesektetServiceRules.LessonStat(lesson),
                application = MesektetServiceRules.LessonApplication(lesson),
                value = MesektetServiceRules.LessonValue(lesson),
                durationSeconds = MesektetServiceRules.LessonSeconds,
                isDebuff = false,
            }, LessonSource);
            _lessonLit = lesson;
            _lessonUntil = Time.time + MesektetServiceRules.LessonSeconds;
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                SayAbove(CityTrainingTexts.Get(localization, CityTrainingTexts.LessonLine(cousin.Value)));
            }

            return CityTrainingTexts.Format(localization, CityTrainingTexts.LessonDone, CityTrainingTexts.Get(localization, CityTrainingTexts.LessonName(lesson)));
        }

        private sealed class LessonScreen : ICityServiceScreen
        {
            private readonly MesektetServicesMechanicsDriver _driver;

            public LessonScreen(MesektetServicesMechanicsDriver driver) => _driver = driver;

            public string Title(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.LessonTitle);

            public string Intro(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.LessonIntro);

            public string Summary(LocalizationManager l)
            {
                float left = _driver.LessonLeft;
                return left > 0f
                    ? CityTrainingTexts.Format(l, CityTrainingTexts.LessonActive, CityTrainingTexts.Get(l, CityTrainingTexts.LessonName(_driver._lessonLit)),
                        MesektetServiceRules.MinutesLeft(left))
                    : CityTrainingTexts.Get(l, CityTrainingTexts.LessonNone);
            }

            public IReadOnlyList<CityServiceRow> Rows(LocalizationManager l)
            {
                WorldMapProgress progress = Progress();
                long freeDay = progress != null ? progress.GetDailyMark(MesektetServiceRules.LessonDailyKey) : 0;
                int cost = MesektetServiceRules.LessonCost(freeDay, Today());
                string button = cost == 0
                    ? CityTrainingTexts.Get(l, CityTrainingTexts.LessonButtonFree)
                    : CityTrainingTexts.Format(l, CityTrainingTexts.LessonButtonGold, cost);
                bool lit = _driver.LessonLeft > 0f;
                var rows = new List<CityServiceRow>();
                foreach (DjediLesson lesson in MesektetServiceRules.Lessons)
                {
                    rows.Add(new CityServiceRow(CityTrainingTexts.Get(l, CityTrainingTexts.LessonName(lesson)),
                        CityTrainingTexts.Get(l, CityTrainingTexts.LessonEffect(lesson)), button, _driver.Gold >= cost,
                        lit && lesson == _driver._lessonLit));
                }

                return rows;
            }

            public string Pick(int row, LocalizationManager l) =>
                row >= 0 && row < MesektetServiceRules.Lessons.Length ? _driver.Learn(MesektetServiceRules.Lessons[row], l) : string.Empty;

            public string CloseLabel(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.Close);
        }

        // ------------------------------------------------------------------
        // Setne's shelf of the Caderno
        // ------------------------------------------------------------------

        private static int PagesFilled() => ServiceLocator.TryGet(out BestiaryProgress bestiary) && bestiary != null ? bestiary.UnlockedCount : 0;

        private static int ShelvesClaimed()
        {
            WorldMapProgress progress = Progress();
            return progress != null ? (int)Math.Max(0L, progress.GetTimeMark(MesektetServiceRules.ShelfClaimedKey)) : 0;
        }

        /// <summary>The (item id, amount) of one shelf, gold first.</summary>
        private static List<(string Id, int Amount)> ShelfRewards(int shelf)
        {
            var list = new List<(string, int)> { (RefineItems.Gold, MesektetServiceRules.ShelfGold(shelf)) };
            if (MesektetServiceRules.ShelfBlessingStones(shelf) > 0) list.Add((RefineItems.BlessingStone, MesektetServiceRules.ShelfBlessingStones(shelf)));
            if (MesektetServiceRules.ShelfSoulStones(shelf) > 0) list.Add((RefineItems.SoulStone, MesektetServiceRules.ShelfSoulStones(shelf)));
            if (MesektetServiceRules.ShelfLifeStones(shelf) > 0) list.Add((RefineItems.LifeStone, MesektetServiceRules.ShelfLifeStones(shelf)));
            return list;
        }

        private static string RewardText(int shelf, LocalizationManager l)
        {
            var parts = new List<string>();
            foreach ((string id, int amount) in ShelfRewards(shelf))
            {
                if (id == RefineItems.Gold)
                {
                    parts.Add(CityTrainingTexts.Format(l, CityTrainingTexts.RewardGold, amount));
                    continue;
                }

                ItemDefinition item = ItemDatabase.GetById(id);
                parts.Add(CityTrainingTexts.Format(l, CityTrainingTexts.RewardItem, item != null ? item.LocalizedName(l) : id, amount));
            }

            return string.Join(" · ", parts);
        }

        /// <summary>Receives the next shelf (all its rewards or nothing): the status line for the screen.</summary>
        private string Claim(int shelf, LocalizationManager l)
        {
            MesektetServiceCheck check = MesektetServiceRules.CheckShelf(shelf, PagesFilled(), ShelvesClaimed());
            if (check == MesektetServiceCheck.WaitTurn)
            {
                return CityTrainingTexts.Get(l, CityTrainingTexts.ShelfWaitTurn);
            }

            if (check != MesektetServiceCheck.Ready || _bag == null)
            {
                return CityTrainingTexts.Get(l, CityTrainingTexts.ShelfNothing);
            }

            // All or nothing: every stack must fit before anything is handed over.
            List<(string Id, int Amount)> rewards = ShelfRewards(shelf);
            var given = new List<(ItemDefinition Item, int Amount)>();
            foreach ((string id, int amount) in rewards)
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                if (item == null)
                {
                    continue;
                }

                int left = _bag.AddItem(item, amount);
                given.Add((item, amount - left));
                if (left > 0)
                {
                    foreach ((ItemDefinition back, int count) in given)
                    {
                        if (count > 0)
                        {
                            _bag.RemoveById(back.ItemId, count);
                        }
                    }

                    return CityTrainingTexts.Get(l, CityTrainingTexts.BagFull);
                }
            }

            WorldMapProgress progress = Progress();
            progress?.SetTimeMark(MesektetServiceRules.ShelfClaimedKey, shelf + 1);
            CharacterId? cousin = Hero();
            if (cousin.HasValue)
            {
                SayAbove(CityTrainingTexts.Get(l, CityTrainingTexts.ShelfLine(cousin.Value)));
            }

            return CityTrainingTexts.Format(l, CityTrainingTexts.ShelfReceived, RewardText(shelf, l));
        }

        private sealed class ShelfScreen : ICityServiceScreen
        {
            private readonly MesektetServicesMechanicsDriver _driver;

            public ShelfScreen(MesektetServicesMechanicsDriver driver) => _driver = driver;

            public string Title(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.ShelfTitle);

            public string Intro(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.ShelfIntro);

            public string Summary(LocalizationManager l) => CityTrainingTexts.Format(l, CityTrainingTexts.ShelfPages, PagesFilled(), BestiaryCatalog.Count);

            public IReadOnlyList<CityServiceRow> Rows(LocalizationManager l)
            {
                int pages = PagesFilled();
                int claimed = ShelvesClaimed();
                var rows = new List<CityServiceRow>();
                for (int shelf = 0; shelf < MesektetServiceRules.ShelfCount; shelf++)
                {
                    string title = CityTrainingTexts.Format(l, CityTrainingTexts.ShelfRow, MesektetServiceRules.ShelfPages[shelf]);
                    string reward = RewardText(shelf, l);
                    if (shelf < claimed)
                    {
                        rows.Add(new CityServiceRow(title, reward + " · " + CityTrainingTexts.Get(l, CityTrainingTexts.ShelfClaimed), null, false));
                        continue;
                    }

                    MesektetServiceCheck check = MesektetServiceRules.CheckShelf(shelf, pages, claimed);
                    if (check == MesektetServiceCheck.NothingToClaim)
                    {
                        rows.Add(new CityServiceRow(title, reward + " · " + CityTrainingTexts.Format(l, CityTrainingTexts.ShelfMissing,
                            MesektetServiceRules.PagesToGo(shelf, pages)), null, false));
                        continue;
                    }

                    rows.Add(new CityServiceRow(title, reward, CityTrainingTexts.Get(l, CityTrainingTexts.ShelfClaimButton),
                        check == MesektetServiceCheck.Ready, check == MesektetServiceCheck.Ready));
                }

                return rows;
            }

            public string Pick(int row, LocalizationManager l) => _driver.Claim(row, l);

            public string CloseLabel(LocalizationManager l) => CityTrainingTexts.Get(l, CityTrainingTexts.Close);
        }

        // ------------------------------------------------------------------
        // Feedback
        // ------------------------------------------------------------------

        private void Say(string key)
        {
            if (_player == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager manager);
            CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * FeedbackHeight, CityTrainingTexts.Get(manager, key));
        }

        private void SayAbove(string text)
        {
            if (_player != null)
            {
                CombatFxManager.SpawnLootWarningText(_player.position + Vector3.up * (FeedbackHeight + 0.7f), text);
            }
        }
    }
}
