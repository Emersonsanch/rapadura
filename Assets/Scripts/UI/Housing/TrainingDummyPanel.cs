using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Housing
{
    /// <summary>One attack the training panel offers: the move the rule simulates plus what to call it.</summary>
    public readonly struct TrainingMoveOption
    {
        public readonly TrainingMove Move;

        /// <summary>Null for the basic attack.</summary>
        public readonly SkillDefinition Skill;

        public readonly int SkillLevel;

        public TrainingMoveOption(TrainingMove move, SkillDefinition skill, int skillLevel)
        {
            Move = move;
            Skill = skill;
            SkillLevel = skillLevel;
        }
    }

    /// <summary>The active cousin as the dummy sees them: level, loadout and the attacks to try.</summary>
    public sealed class TrainingDummyContext
    {
        public CharacterId? Hero;
        public int Level = 1;
        public TrainingLoadout Loadout = new TrainingLoadout();
        public readonly List<TrainingMoveOption> Moves = new List<TrainingMoveOption>();
    }

    /// <summary>
    /// Reads the active cousin's numbers for the training dummy (<see cref="TrainingDummyRules"/>). With a live hero in the
    /// scene it uses its BuffController (gear, attributes and buffs, exactly like the arena); on the map (no live hero)
    /// it uses the carried save: level (PlayerStats), Destreza / Poder Arcano (AttributeSet) and the learned skills'
    /// levels (SkillManager). Balance numbers come from the shipped PlayerCombatBalance / ProgressionCurve assets.
    /// </summary>
    public static class TrainingDummyLoadoutBuilder
    {
        public const string PlayerStatsKey = "PlayerStats";
        public const string AttributeSetKey = "AttributeSet";
        public const string SkillManagerKey = "SkillManager";

        /// <summary>PlayerBasicAttack's default swing cooldown (its inspector value lives in the arena scene).</summary>
        public const float DefaultBasicAttackCooldown = 0.5f;

        /// <summary>PlayerBasicAttack's fallback damage when no balance asset exists.</summary>
        public const float DefaultBasicAttackDamage = 15f;

        public static TrainingDummyContext Build(CharacterId? hero)
        {
            var context = new TrainingDummyContext { Hero = hero };
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            ProgressionCurve curve = ProgressionCurve.LoadDefault();
            float basicDamage = balance != null ? balance.BasicAttackDamage : DefaultBasicAttackDamage;
            float basicInterval;
            SkillManagerSaveState skills = null;

            GameObject player = FindPlayer();
            PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
            if (stats != null)
            {
                context.Level = Mathf.Max(1, stats.Level);
                BuffController buffs = player.GetComponent<BuffController>();
                context.Loadout = new TrainingLoadout
                {
                    LevelMultiplier = curve != null ? curve.GetDamageMultiplier(context.Level) : 1f,
                    FlatAttackDamage = buffs != null ? buffs.GetFlatTotal(StatType.AttackDamage) : 0f,
                    AttackDamagePercent = buffs != null ? buffs.GetModifiedValue(StatType.AttackDamage, 1f) - buffs.GetModifiedValue(StatType.AttackDamage, 0f) - 1f : 0f,
                    CriticalChance = OutgoingDamage.GetCriticalChance(buffs, balance),
                    CriticalMultiplier = OutgoingDamage.GetCriticalMultiplier(buffs, balance),
                };
                PlayerBasicAttack basic = player.GetComponent<PlayerBasicAttack>();
                basicInterval = basic != null
                    ? basic.EffectiveCooldown
                    : PlayerBasicAttack.ResolveCooldown(DefaultBasicAttackCooldown, buffs != null ? buffs.GetAdditiveFractionTotal(StatType.AttackSpeed) : 0f, HeroActionTiming.StrikeDelay);
                SkillManager manager = player.GetComponent<SkillManager>();
                skills = manager != null ? CaptureLevels(manager) : null;
            }
            else
            {
                context.Level = RomariaOratoryHub.CarriedLevel();
                AttributeSetSaveState attributes = Carried<AttributeSetSaveState>(AttributeSetKey);
                context.Loadout = TrainingDummyRules.FromAttributes(
                    curve != null ? curve.GetDamageMultiplier(context.Level) : 1f,
                    attributes != null ? attributes.dexterity : 0,
                    attributes != null ? attributes.arcanePower : 0,
                    balance != null ? balance.BaseCriticalChance : 0f,
                    balance != null ? balance.BaseCriticalMultiplier : PlayerCombatBalance.DefaultCriticalMultiplier);
                basicInterval = PlayerBasicAttack.ResolveCooldown(DefaultBasicAttackCooldown, 0f, HeroActionTiming.StrikeDelay);
                skills = Carried<SkillManagerSaveState>(SkillManagerKey);
            }

            IReadOnlyList<SkillDefinition> heroSkills = hero.HasValue ? CharacterSkillSet.GetSkills(hero.Value) : Array.Empty<SkillDefinition>();
            context.Moves.AddRange(BuildMoves(heroSkills, skills, context.Level, basicDamage, basicInterval));
            return context;
        }

        /// <summary>
        /// The basic attack first, then every damaging skill the hero can use: with a skill book, the learned ones at their
        /// level; without one (a brand-new hero), the ones open at <paramref name="heroLevel"/> with no prerequisite, at
        /// level 1 (what <see cref="CharacterSkillSet.GrantTo"/> learns).
        /// </summary>
        public static List<TrainingMoveOption> BuildMoves(IReadOnlyList<SkillDefinition> heroSkills, SkillManagerSaveState book, int heroLevel,
            float basicDamage, float basicInterval)
        {
            var moves = new List<TrainingMoveOption>
            {
                new TrainingMoveOption(new TrainingMove("basic_attack", basicDamage, basicInterval, true), null, 0),
            };

            if (heroSkills == null)
            {
                return moves;
            }

            foreach (SkillDefinition skill in heroSkills)
            {
                if (skill == null)
                {
                    continue;
                }

                int level = SkillLevel(skill, book, heroLevel);
                if (level <= 0)
                {
                    continue;
                }

                float damage = skill.GetDamageForLevel(level);
                if (damage <= 0f)
                {
                    continue;
                }

                moves.Add(new TrainingMoveOption(
                    TrainingMove.ForSkill(skill.SkillId, damage, skill.CooldownSeconds, skill.CastTimeSeconds, skill.RecoverySeconds), skill, level));
            }

            return moves;
        }

        /// <summary>The skill's level for the dummy, or 0 when the hero can not use it yet.</summary>
        public static int SkillLevel(SkillDefinition skill, SkillManagerSaveState book, int heroLevel)
        {
            if (skill == null)
            {
                return 0;
            }

            if (book != null && book.skills != null && book.skills.Length > 0)
            {
                foreach (LearnedSkillSaveEntry entry in book.skills)
                {
                    if (entry != null && entry.skillId == skill.SkillId)
                    {
                        return Mathf.Max(1, entry.level);
                    }
                }

                return 0;
            }

            SkillRequirement requirement = skill.Requirement;
            bool open = requirement == null
                || (requirement.minimumPlayerLevel <= Mathf.Max(1, heroLevel) && (requirement.requiredSkills == null || requirement.requiredSkills.Length == 0));
            return open ? 1 : 0;
        }

        private static SkillManagerSaveState CaptureLevels(SkillManager manager)
        {
            var entries = new List<LearnedSkillSaveEntry>();
            foreach (SkillInstance instance in manager.LearnedSkills)
            {
                if (instance?.Definition != null)
                {
                    entries.Add(new LearnedSkillSaveEntry { skillId = instance.Definition.SkillId, level = instance.Level });
                }
            }

            return new SkillManagerSaveState { skills = entries.ToArray() };
        }

        private static GameObject FindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag("Player");
            }
            catch (UnityException)
            {
                return null;
            }
        }

        private static T Carried<T>(string key) where T : class
        {
            if (!ServiceLocator.TryGet(out SaveManager saves) || !saves.TryGetCarriedJson(key, out string json) || string.IsNullOrEmpty(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// "Boneco de treino de palha" panel (Docs/Housing.md §11.8): opened by tapping the dummy in "Minha casa". The active
    /// cousin picks the basic attack or a learned attack skill and the panel shows a <see cref="TrainingDummyRules.SessionSeconds"/>
    /// session: hits, damage per hit (normal / critical), crits and damage per second, plus the cousin's line. No XP, gold,
    /// loot or mana: nothing is granted or spent. Built in code on the house overlay with the <c>house-life*</c> look.
    /// </summary>
    public sealed class TrainingDummyPanel
    {
        public const string PanelName = "wm-house-dummy";
        public const int HitsShown = 12;

        private readonly VisualElement _root;
        private readonly Label _title;
        private readonly Button _close;
        private readonly ScrollView _scroll;
        private readonly Func<LocalizationManager> _localization;
        private readonly Action<string, bool> _status;
        private readonly Action _closed;
        private readonly System.Random _random = new System.Random();

        private TrainingDummyContext _context;
        private TrainingSessionResult _last;
        private TrainingMoveOption _lastOption;

        public TrainingDummyPanel(VisualElement parent, Func<LocalizationManager> localization, Action<string, bool> status, Action closed)
        {
            _localization = localization;
            _status = status;
            _closed = closed;

            _root = new VisualElement { name = PanelName };
            _root.AddToClassList("house-life");
            _root.style.display = DisplayStyle.None;
            var header = new VisualElement();
            header.AddToClassList("house-life-header");
            _title = new Label();
            _title.AddToClassList("house-life-title");
            header.Add(_title);
            _close = new Button(Hide) { name = "wm-house-dummy-close", focusable = true };
            _close.AddToClassList("rap-button");
            _close.AddToClassList("house-life-close");
            header.Add(_close);
            _root.Add(header);
            _scroll = new ScrollView(ScrollViewMode.Vertical) { name = "wm-house-dummy-scroll" };
            _scroll.AddToClassList("house-life-scroll");
            _root.Add(_scroll);
            parent?.Add(_root);
        }

        public bool IsVisible { get; private set; }
        public VisualElement Root => _root;
        public TrainingSessionResult LastSession => _last;

        private LocalizationManager L => _localization?.Invoke();

        public void Show()
        {
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            _context = TrainingDummyLoadoutBuilder.Build(hero);
            _last = null;
            IsVisible = true;
            _root.style.display = DisplayStyle.Flex;
            Refresh();
            _close.Focus();
        }

        public void Hide()
        {
            if (!IsVisible) return;
            IsVisible = false;
            _root.style.display = DisplayStyle.None;
            _closed?.Invoke();
        }

        public void Refresh()
        {
            if (!IsVisible || _context == null) return;
            LocalizationManager l = L;
            _title.text = HouseLifeTexts.Get(l, HouseLifeTexts.DummyTitle);
            _close.text = HouseLifeTexts.Get(l, HouseLifeTexts.Close);
            _scroll.Clear();

            Line(HouseLifeTexts.Get(l, HouseLifeTexts.Dummy));
            if (!_context.Hero.HasValue)
            {
                Line(HouseLifeTexts.Get(l, HouseLifeTexts.DummyNoHero));
                return;
            }

            Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyHero, WorldMapHudPresenter.HeroName(l, _context.Hero), _context.Level), "house-section");
            Line(HouseLifeTexts.Get(l, HouseLifeTexts.DummyPick));
            for (int i = 0; i < _context.Moves.Count; i++)
            {
                TrainingMoveOption option = _context.Moves[i];
                Button button = AddButton("wm-house-dummy-move-" + i, MoveName(l, option), () => Train(option));
                button.EnableInClassList("house-tool--active", _last != null && _lastOption.Move.Id == option.Move.Id);
            }

            if (_context.Moves.Count <= 1)
            {
                Line(HouseLifeTexts.Get(l, HouseLifeTexts.DummyNoSkills), "house-life-line--dim");
            }

            if (_last != null)
            {
                Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyResultHeader, MoveName(l, _lastOption), Number(_last.Seconds)), "house-section");
                Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyPerHit, _last.HitCount, Number(_last.AverageHit), _last.NormalHit, _last.CriticalHit));
                Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyCrits, _last.CriticalHits, Number(_last.CriticalRate * 100f),
                    Number(Mathf.Clamp01(_context.Loadout.CriticalChance) * 100f)));
                Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyDps, Number(_last.DamagePerSecond), _last.TotalDamage), "house-life-row--met");
                Line(HouseLifeTexts.Format(l, HouseLifeTexts.DummyLastHits, FormatHits(_last.Hits, HitsShown)), "house-life-line--dim");
            }

            Line(HouseLifeTexts.Get(l, HouseLifeTexts.DummyNoXp), "house-life-line--dim");
            Line(HouseLifeTexts.Get(l, HouseLifeTexts.DummySourceNote), "house-life-line--dim");
        }

        private void Train(TrainingMoveOption option)
        {
            _lastOption = option;
            _last = TrainingDummyRules.Simulate(option.Move, _context.Loadout, _random);
            LocalizationManager l = L;
            _status?.Invoke(HouseLifeTexts.Format(l, HouseLifeTexts.DummyReactionKey(_context.Hero), Number(_last.DamagePerSecond)), false);
            Refresh();
        }

        /// <summary>"18 · 27! · 18 …": the first <paramref name="max"/> hits, a "!" on the crits.</summary>
        public static string FormatHits(IReadOnlyList<TrainingHit> hits, int max)
        {
            if (hits == null || hits.Count == 0) return "—";
            var text = new StringBuilder();
            int shown = Math.Min(Math.Max(1, max), hits.Count);
            for (int i = 0; i < shown; i++)
            {
                if (i > 0) text.Append(" · ");
                text.Append(hits[i].Damage.ToString(CultureInfo.InvariantCulture));
                if (hits[i].Critical) text.Append('!');
            }

            if (hits.Count > shown) text.Append(" …");
            return text.ToString();
        }

        /// <summary>Whole numbers as they are, others with one decimal (invariant digits, like the other damage numbers).</summary>
        public static string Number(float value) => Math.Round(value, 1).ToString("0.#", CultureInfo.InvariantCulture);

        private static string MoveName(LocalizationManager l, TrainingMoveOption option)
        {
            return option.Skill == null
                ? HouseLifeTexts.Get(l, HouseLifeTexts.DummyBasicAttack)
                : HouseLifeTexts.Format(l, HouseLifeTexts.DummySkill, option.Skill.LocalizedName(l), option.SkillLevel);
        }

        private void Line(string text, string extraClass = null)
        {
            var label = new Label(text ?? string.Empty);
            label.AddToClassList("house-life-line");
            if (!string.IsNullOrEmpty(extraClass)) label.AddToClassList(extraClass);
            _scroll.Add(label);
        }

        private Button AddButton(string name, string text, Action onClick)
        {
            var button = new Button(onClick) { name = name, text = text, focusable = true };
            button.AddToClassList("rap-button");
            button.AddToClassList("house-life-button");
            _scroll.Add(button);
            return button;
        }
    }
}
