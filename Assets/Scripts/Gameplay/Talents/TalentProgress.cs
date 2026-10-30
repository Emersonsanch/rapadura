using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Talents
{
    /// <summary>JSON-serializable snapshot of <see cref="TalentProgress"/> (one record per cousin).</summary>
    [Serializable]
    public class TalentSaveState
    {
        public List<TalentRecord> heroes = new List<TalentRecord>();
    }

    /// <summary>Raised when a cousin's proficiency goes up a level.</summary>
    public readonly struct ProficiencyLevelUpEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly ProficiencyKind Kind;
        public readonly int Level;

        public ProficiencyLevelUpEvent(CharacterId hero, ProficiencyKind kind, int level)
        {
            Hero = hero;
            Kind = kind;
            Level = level;
        }
    }

    /// <summary>Raised after a wheel point is spent or the wheel is reset (UI refresh).</summary>
    public readonly struct TalentWheelChangedEvent : IGameEvent
    {
        public readonly CharacterId Hero;

        public TalentWheelChangedEvent(CharacterId hero)
        {
            Hero = hero;
        }
    }

    /// <summary>
    /// Per-save training and "Roda de Talentos" of each cousin (Docs/Balance/README.md §26). Plain C# like the other progress
    /// managers (built by GameManager, saved under <see cref="Key"/>, NewSessionReset; an older save has no entry and starts
    /// untrained). Counts tries from the combat events (swing in a fight, mana spent, hit taken) for the cousin playing and
    /// puts the trained levels and the wheel on the hero's <see cref="BuffController"/> under <see cref="ProficiencySourceKey"/>
    /// and <see cref="WheelSourceKey"/> (re-applied after scene loads by <see cref="TalentPassiveApplier"/>).
    /// Rules live in <see cref="ProficiencyRules"/> / <see cref="TalentWheelRules"/>.
    /// </summary>
    public class TalentProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Talents";
        public const string ProficiencySourceKey = "talent_proficiency";
        public const string WheelSourceKey = "talent_wheel";
        public const string GoldItemId = "item_gold";
        private const string PlayerTag = "Player";

        private readonly Dictionary<CharacterId, TalentRecord> _records = new Dictionary<CharacterId, TalentRecord>();
        private double _lastCombatTime = double.NegativeInfinity;
        private bool _subscribed;
        private static TalentProgress _standalone;

        public string SaveKey => Key;

        /// <summary>Clock of the combat window (tests swap it; real time otherwise).</summary>
        public Func<double> Clock { get; set; } = () => Time.timeAsDouble;

        public TalentProgress()
        {
            ResetForNewSession();
        }

        public static TalentProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out TalentProgress progress))
            {
                return progress;
            }

            return _standalone ??= new TalentProgress();
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                TalentTexts.EnsureRegistered(localization);
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Subscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Subscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttackPerformed);
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void ResetForNewSession()
        {
            _records.Clear();
            _lastCombatTime = double.NegativeInfinity;
            foreach (CharacterId hero in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                _records[hero] = TalentWheelRules.NewRecord(hero);
            }
        }

        // ------------------------------------------------------------------ queries

        public TalentRecord Record(CharacterId hero) => _records.TryGetValue(hero, out TalentRecord record) ? record : null;

        public ProficiencyEntry Proficiency(CharacterId hero, ProficiencyKind kind) => TalentWheelRules.Proficiency(Record(hero), kind);

        public int AvailablePoints(CharacterId hero, int heroLevel) => TalentWheelRules.AvailablePoints(Record(hero), heroLevel);

        // ------------------------------------------------------------------ training

        /// <summary>Adds tries to <paramref name="hero"/>'s <paramref name="kind"/>; re-applies the bonus on a level up. Returns levels gained.</summary>
        public int Train(CharacterId hero, ProficiencyKind kind, long tries, BuffController buffs = null)
        {
            ProficiencyEntry entry = Proficiency(hero, kind);
            int gained = ProficiencyRules.AddTries(entry, tries);
            if (gained > 0)
            {
                ApplyProficiencyModifiers(buffs, hero);
                EventBus.Publish(new ProficiencyLevelUpEvent(hero, kind, entry.level));
            }

            return gained;
        }

        /// <summary>Marks the hero as fighting at <paramref name="now"/> (a swing trains within the window).</summary>
        public void MarkCombat(double now) => _lastCombatTime = now;

        /// <summary>One basic swing at <paramref name="now"/>: one Golpe try when the hero is in a fight. True when it trained.</summary>
        public bool HandleSwing(CharacterId hero, double now, BuffController buffs = null)
        {
            if (!ProficiencyRules.IsInCombat(now, _lastCombatTime))
            {
                return false;
            }

            Train(hero, ProficiencyKind.Weapon, 1, buffs);
            return true;
        }

        // ------------------------------------------------------------------ wheel

        public TalentSpendResult CanSpend(CharacterId hero, int heroLevel, string nodeId) =>
            TalentWheelRules.CanSpend(Record(hero), hero, heroLevel, nodeId);

        public TalentSpendResult Spend(CharacterId hero, int heroLevel, string nodeId, BuffController buffs = null)
        {
            TalentSpendResult result = TalentWheelRules.Spend(Record(hero), hero, heroLevel, nodeId);
            if (result == TalentSpendResult.Spent)
            {
                ApplyWheelModifiers(buffs, hero);
                EventBus.Publish(new TalentWheelChangedEvent(hero));
            }

            return result;
        }

        public int RespecCost(CharacterId hero) => TalentWheelRules.RespecCost(Record(hero));

        /// <summary>
        /// Resets the wheel, taking <see cref="RespecCost"/> gold (<see cref="GoldItemId"/>) from <paramref name="bag"/>.
        /// <paramref name="refunded"/> = points given back.
        /// </summary>
        public TalentRespecResult Respec(CharacterId hero, Rapadura.Gameplay.Inventory.InventoryManager bag, out int refunded, BuffController buffs = null)
        {
            refunded = 0;
            TalentRecord record = Record(hero);
            int cost = TalentWheelRules.RespecCost(record);
            int gold = bag != null ? bag.GetTotalCount(GoldItemId) : 0;
            TalentRespecResult check = TalentWheelRules.CanRespec(record, gold);
            if (check != TalentRespecResult.Done)
            {
                return check;
            }

            if (bag == null || !bag.RemoveById(GoldItemId, cost))
            {
                return TalentRespecResult.NotEnoughGold;
            }

            refunded = TalentWheelRules.Reset(record);
            ApplyWheelModifiers(buffs, hero);
            EventBus.Publish(new TalentWheelChangedEvent(hero));
            return TalentRespecResult.Done;
        }

        // ------------------------------------------------------------------ applying to the hero

        /// <summary>Trained levels + wheel of <paramref name="hero"/> on <paramref name="buffs"/> (replaces the previous ones).</summary>
        public void ApplyTo(BuffController buffs, CharacterId hero)
        {
            if (buffs == null)
            {
                return;
            }

            ApplyProficiencyModifiers(buffs, hero);
            ApplyWheelModifiers(buffs, hero);
        }

        /// <summary>Re-applies the playing cousin's bonuses on the scene's Player (no-op without one).</summary>
        public void ApplyToPlayer()
        {
            GameObject player = FindPlayer();
            if (player != null && player.TryGetComponent(out BuffController buffs) && TryActiveHero(out CharacterId hero))
            {
                ApplyTo(buffs, hero);
            }
        }

        private void ApplyProficiencyModifiers(BuffController buffs, CharacterId hero)
        {
            if (buffs == null && !TryPlayerBuffs(out buffs))
            {
                return;
            }

            buffs.RemoveEffectsBySource(ProficiencySourceKey);
            List<StatModifierDefinition> modifiers = ProficiencyRules.Modifiers(Record(hero)?.proficiencies);
            if (modifiers.Count > 0)
            {
                buffs.ApplyEffects(modifiers, ProficiencySourceKey);
            }
        }

        private void ApplyWheelModifiers(BuffController buffs, CharacterId hero)
        {
            if (buffs == null && !TryPlayerBuffs(out buffs))
            {
                return;
            }

            buffs.RemoveEffectsBySource(WheelSourceKey);
            List<StatModifierDefinition> modifiers = TalentWheelRules.Modifiers(Record(hero));
            if (modifiers.Count > 0)
            {
                buffs.ApplyEffects(modifiers, WheelSourceKey);
            }
        }

        // ------------------------------------------------------------------ events

        private static bool IsPlayer(GameObject go) => go != null && (go.CompareTag(PlayerTag) || (go.transform.root != null && go.transform.root.CompareTag(PlayerTag)));

        private void OnAttackPerformed(PlayerAttackPerformedEvent evt)
        {
            if (IsPlayer(evt.Attacker) && TryActiveHero(out CharacterId hero))
            {
                HandleSwing(hero, Clock());
            }
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            if (evt.FinalDamage <= 0f)
            {
                return;
            }

            bool playerHit = IsPlayer(evt.Target);
            if (playerHit || IsPlayer(evt.Source))
            {
                MarkCombat(Clock());
            }

            if (playerHit && !IsPlayer(evt.Source) && TryActiveHero(out CharacterId hero))
            {
                Train(hero, ProficiencyKind.Shielding, 1);
            }
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Skill == null || !IsPlayer(evt.Caster) || !TryActiveHero(out CharacterId hero))
            {
                return;
            }

            long tries = ProficiencyRules.MagicTries(evt.Skill.ManaCost);
            if (tries > 0)
            {
                Train(hero, ProficiencyKind.Magic, tries);
            }
        }

        private void OnCharacterSelected(CharacterSelectedEvent evt) => ApplyToPlayer();

        private void OnCharacterRestored(CharacterSelectionRestoredEvent evt) => ApplyToPlayer();

        private void OnGameLoaded(GameLoadedEvent evt) => ApplyToPlayer();

        private static bool TryPlayerBuffs(out BuffController buffs)
        {
            buffs = null;
            GameObject player = FindPlayer();
            return player != null && player.TryGetComponent(out buffs);
        }

        private static GameObject FindPlayer()
        {
            try
            {
                return GameObject.FindGameObjectWithTag(PlayerTag);
            }
            catch (UnityException)
            {
                return null;
            }
        }

        public static bool TryActiveHero(out CharacterId hero)
        {
            hero = default;
            if (ServiceLocator.TryGet(out CharacterSelectionState selection) && selection != null && selection.Selected.HasValue)
            {
                hero = selection.Selected.Value;
                return true;
            }

            return false;
        }

        // ------------------------------------------------------------------ save

        public object CaptureState()
        {
            var state = new TalentSaveState();
            foreach (CharacterId hero in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                TalentRecord record = Record(hero);
                var copy = new TalentRecord { characterId = hero.ToString(), respecs = record.respecs };
                foreach (ProficiencyEntry entry in record.proficiencies)
                {
                    copy.proficiencies.Add(new ProficiencyEntry { kind = entry.kind, level = entry.level, tries = entry.tries });
                }

                foreach (TalentNodeRank rank in record.nodes)
                {
                    copy.nodes.Add(new TalentNodeRank { nodeId = rank.nodeId, rank = rank.rank });
                }

                state.heroes.Add(copy);
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not TalentSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            if (saved.heroes == null)
            {
                return;
            }

            foreach (TalentRecord entry in saved.heroes)
            {
                if (entry == null || !Enum.TryParse(entry.characterId, out CharacterId hero) || !Enum.IsDefined(typeof(CharacterId), hero))
                {
                    continue;
                }

                var record = new TalentRecord
                {
                    characterId = hero.ToString(),
                    respecs = entry.respecs,
                    proficiencies = entry.proficiencies != null ? new List<ProficiencyEntry>(entry.proficiencies) : new List<ProficiencyEntry>(),
                    nodes = entry.nodes != null ? new List<TalentNodeRank>(entry.nodes) : new List<TalentNodeRank>(),
                };
                TalentWheelRules.Sanitize(record, hero);
                _records[hero] = record;
            }
        }
    }
}
