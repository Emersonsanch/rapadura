using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.ClassEvolution
{
    /// <summary>JSON-serializable snapshot of <see cref="ClassEvolutionProgress"/> (one record per cousin).</summary>
    [Serializable]
    public class ClassEvolutionSaveState
    {
        public List<GraduationRecord> heroes = new List<GraduationRecord>();
    }

    /// <summary>Raised when a cousin graduates (tier 1..3).</summary>
    public readonly struct ClassGraduatedEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly GraduationSpec Graduation;

        public ClassGraduatedEvent(CharacterId hero, GraduationSpec graduation)
        {
            Hero = hero;
            Graduation = graduation;
        }
    }

    /// <summary>Raised when a graduation mission counted a kill or was accepted (HUD/journal refresh).</summary>
    public readonly struct GraduationMissionChangedEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly GraduationMissionStatus Status;

        public GraduationMissionChangedEvent(CharacterId hero, GraduationMissionStatus status)
        {
            Hero = hero;
            Status = status;
        }
    }

    /// <summary>Raised on every master level gained.</summary>
    public readonly struct MasterLevelGainedEvent : IGameEvent
    {
        public readonly CharacterId Hero;
        public readonly int MasterLevel;

        public MasterLevelGainedEvent(CharacterId hero, int masterLevel)
        {
            Hero = hero;
            MasterLevel = masterLevel;
        }
    }

    /// <summary>Raised when a class combo completes (bonus already applied).</summary>
    public readonly struct ClassComboCompletedEvent : IGameEvent
    {
        public readonly GameObject Caster;
        public readonly ClassComboSpec Combo;

        public ClassComboCompletedEvent(GameObject caster, ClassComboSpec combo)
        {
            Caster = caster;
            Combo = combo;
        }
    }

    /// <summary>
    /// Per-save class evolution (Docs/ClassEvolution.md): graduation tier, the accepted mission, master level/XP and master
    /// tree of each cousin. Plain C# like the other progress managers (built by GameManager, registered with SaveManager under
    /// <see cref="Key"/> and NewSessionReset; an older save has no entry and loads at tier 0). Applies the state to the hero
    /// on the field: graduation skills on the <see cref="SkillManager"/>, master-tree modifiers on the <see cref="BuffController"/>,
    /// the look (<see cref="ClassEvolutionVisuals"/>), counts mission kills, runs the class combo and spreads group buffs to
    /// allies around (<see cref="PartySupportRules"/>). Rules live in <see cref="ClassEvolutionRules"/>/<see cref="MasterLevelRules"/>.
    /// </summary>
    public class ClassEvolutionProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "ClassEvolution";
        public const string MasterSourceKey = "class_master_tree";
        public const string ComboSourcePrefix = "class_combo_";
        public const string GroupBuffSourcePrefix = "class_group_";
        private const string PlayerTag = "Player";

        private readonly Dictionary<CharacterId, GraduationRecord> _records = new Dictionary<CharacterId, GraduationRecord>();
        private readonly Dictionary<GameObject, ClassComboTracker> _combos = new Dictionary<GameObject, ClassComboTracker>();
        private bool _subscribed;
        private static ClassEvolutionProgress _standalone;

        public string SaveKey => Key;

        /// <summary>Clock of the class combo window (tests swap it; real time otherwise).</summary>
        public Func<double> Clock { get; set; } = () => Time.timeAsDouble;

        public ClassEvolutionProgress()
        {
            ResetForNewSession();
        }

        public static ClassEvolutionProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out ClassEvolutionProgress progress))
            {
                return progress;
            }

            return _standalone ??= new ClassEvolutionProgress();
        }

        public void Initialize()
        {
            if (ServiceLocator.TryGet(out LocalizationManager localization))
            {
                ClassEvolutionTexts.EnsureRegistered(localization);
            }

            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
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
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Unsubscribe<SkillCastEvent>(OnSkillCast);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void ResetForNewSession()
        {
            _records.Clear();
            _combos.Clear();
            foreach (CharacterId hero in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                _records[hero] = new GraduationRecord { characterId = hero.ToString() };
            }
        }

        // ------------------------------------------------------------------ queries

        public GraduationRecord Record(CharacterId hero) => _records.TryGetValue(hero, out GraduationRecord record) ? record : null;

        public int Tier(CharacterId hero) => Record(hero)?.tier ?? 0;

        public GraduationMissionStatus Status(CharacterId hero, int heroLevel) => ClassEvolutionRules.Status(hero, Record(hero), heroLevel);

        public bool IsMaster(CharacterId hero, int heroLevel) => MasterLevelRules.IsMaster(Record(hero), heroLevel);

        // ------------------------------------------------------------------ missions

        public bool AcceptMission(CharacterId hero, int heroLevel)
        {
            bool accepted = ClassEvolutionRules.Accept(hero, Record(hero), heroLevel);
            if (accepted)
            {
                EventBus.Publish(new GraduationMissionChangedEvent(hero, Status(hero, heroLevel)));
            }

            return accepted;
        }

        /// <summary>Counts a kill in <paramref name="region"/> for <paramref name="hero"/>'s accepted mission.</summary>
        public bool RecordKill(CharacterId hero, BiomeType? region, int heroLevel)
        {
            bool counted = ClassEvolutionRules.RecordKill(hero, Record(hero), region);
            if (counted)
            {
                EventBus.Publish(new GraduationMissionChangedEvent(hero, Status(hero, heroLevel)));
            }

            return counted;
        }

        /// <summary>Hands the mission in to the mentor; on success re-applies skills/look on <paramref name="heroObject"/> (may be null).</summary>
        public GraduationSpec TurnIn(CharacterId hero, int heroLevel, GameObject heroObject = null)
        {
            GraduationSpec done = ClassEvolutionRules.TurnIn(hero, Record(hero), heroLevel);
            if (done == null)
            {
                return null;
            }

            if (heroObject != null)
            {
                ApplyTo(heroObject, hero, heroLevel);
            }

            EventBus.Publish(new ClassGraduatedEvent(hero, done));
            return done;
        }

        // ------------------------------------------------------------------ master level

        /// <summary>Adds master XP when the hero is a master. Returns master levels gained.</summary>
        public int AddMasterExperience(CharacterId hero, int heroLevel, long amount)
        {
            GraduationRecord record = Record(hero);
            int gained = MasterLevelRules.AddExperience(record, heroLevel, amount);
            for (int i = gained - 1; i >= 0; i--)
            {
                EventBus.Publish(new MasterLevelGainedEvent(hero, record.masterLevel - i));
            }

            return gained;
        }

        public MasterSpendResult CanSpendMasterPoint(CharacterId hero, int heroLevel, string nodeId) =>
            MasterLevelRules.CanSpend(Record(hero), heroLevel, nodeId);

        public MasterSpendResult SpendMasterPoint(CharacterId hero, int heroLevel, string nodeId, BuffController buffs = null)
        {
            MasterSpendResult result = MasterLevelRules.Spend(Record(hero), heroLevel, nodeId);
            if (result == MasterSpendResult.Spent)
            {
                ApplyMasterModifiers(buffs, hero);
            }

            return result;
        }

        public int ResetMasterTree(CharacterId hero, BuffController buffs = null)
        {
            int refunded = MasterLevelRules.Reset(Record(hero));
            ApplyMasterModifiers(buffs, hero);
            return refunded;
        }

        /// <summary>
        /// Called by <c>EnemyRewardGranter</c> with the final kill XP: a master (3rd graduation at level
        /// <see cref="MasterLevelRules.RequiredLevel"/>) puts it all in the master bar instead of normal XP. True when routed.
        /// </summary>
        public static bool TryRouteMasterExperience(PlayerStats killer, int amount)
        {
            if (killer == null || amount <= 0 || !TryActiveHero(out CharacterId hero))
            {
                return false;
            }

            ClassEvolutionProgress progress = ResolveShared();
            if (!progress.IsMaster(hero, killer.Level))
            {
                return false;
            }

            progress.AddMasterExperience(hero, killer.Level, amount);
            return true;
        }

        // ------------------------------------------------------------------ applying to the hero

        /// <summary>Graduation skills, master modifiers and look of <paramref name="hero"/> on <paramref name="heroObject"/>.</summary>
        public void ApplyTo(GameObject heroObject, CharacterId hero, int heroLevel)
        {
            if (heroObject == null)
            {
                return;
            }

            int tier = Tier(hero);
            if (heroObject.TryGetComponent(out SkillManager skills))
            {
                GrantSkills(skills, hero, tier, heroLevel);
            }

            heroObject.TryGetComponent(out BuffController buffs);
            ApplyMasterModifiers(buffs, hero);
            ClassEvolutionVisuals.Apply(heroObject, hero, tier);
            _combos.Remove(heroObject);
        }

        /// <summary>
        /// Takes every other cousin's graduation skills off <paramref name="skills"/> (points spent on extra levels come back,
        /// the unlock was free) and learns + equips (first free slot) this cousin's up to <paramref name="tier"/>. Returns how many were learned.
        /// </summary>
        public static int GrantSkills(SkillManager skills, CharacterId hero, int tier, int heroLevel)
        {
            if (skills == null)
            {
                return 0;
            }

            foreach (GraduationSkillSpec spec in ClassEvolutionCatalog.Skills)
            {
                if (spec.Character == hero && spec.Tier <= tier)
                {
                    continue;
                }

                SkillInstance learned = skills.GetLearnedSkill(spec.SkillId);
                if (learned != null)
                {
                    skills.ForgetSkill(spec.SkillId, Math.Max(0, learned.Level - 1));
                }
            }

            List<SkillDefinition> owned = GraduationSkillFactory.GetAll(ClassEvolutionCatalog.SkillsUpTo(hero, tier));
            skills.RegisterSkills(owned);
            int count = 0;
            foreach (SkillDefinition skill in owned)
            {
                if (skills.IsSkillLearned(skill.SkillId) || skills.LearnSkill(skill, Math.Max(heroLevel, skill.Requirement.minimumPlayerLevel), force: true) == null)
                {
                    continue;
                }

                count++;
                IReadOnlyList<SkillInstance> slots = skills.EquippedSlots;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] == null)
                    {
                        skills.EquipSkill(i, skill.SkillId);
                        break;
                    }
                }
            }

            return count;
        }

        private void ApplyMasterModifiers(BuffController buffs, CharacterId hero)
        {
            if (buffs == null)
            {
                GameObject player = FindPlayer();
                if (player == null || !player.TryGetComponent(out buffs))
                {
                    return;
                }
            }

            buffs.RemoveEffectsBySource(MasterSourceKey);
            List<StatModifierDefinition> modifiers = MasterLevelRules.Modifiers(Record(hero));
            if (modifiers.Count > 0)
            {
                buffs.ApplyEffects(modifiers, MasterSourceKey);
            }
        }

        // ------------------------------------------------------------------ class combo + group buffs

        /// <summary>Feeds one cast to the caster's class combo; applies the bonus when it completes. Public for tests.</summary>
        public bool HandleCast(GameObject caster, CharacterId hero, string skillId, double now)
        {
            if (caster == null || Tier(hero) < ClassEvolutionCatalog.ComboTier)
            {
                return false;
            }

            if (!_combos.TryGetValue(caster, out ClassComboTracker tracker) || tracker.Combo?.Character != hero)
            {
                tracker = new ClassComboTracker(ClassEvolutionCatalog.GetCombo(hero));
                _combos[caster] = tracker;
            }

            if (!tracker.OnCast(skillId, now))
            {
                return false;
            }

            ClassComboSpec combo = tracker.Combo;
            if (caster.TryGetComponent(out BuffController buffs))
            {
                bool damage = combo.BonusType == ComboBonusType.DamageMultiplier;
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = ClassEvolutionTexts.GetFallback(Rapadura.Gameplay.Content.ContentText.ComboNameKey(combo.ComboId)) ?? combo.ComboId,
                    affectedStat = damage ? StatType.AttackDamage : StatType.ResourceCostReduction,
                    application = damage ? ModifierApplication.PercentAdditive : ModifierApplication.Flat,
                    value = combo.BonusValue,
                    durationSeconds = combo.BonusDurationSeconds,
                    isDebuff = false,
                }, ComboSourcePrefix + combo.ComboId);
            }

            EventBus.Publish(new ClassComboCompletedEvent(caster, combo));
            return true;
        }

        /// <summary>Spreads a graduation group buff from <paramref name="caster"/> to other heroes around (the caster's own copy
        /// comes from the skill itself, through SkillManager). Returns how many allies got it.</summary>
        public static int SpreadGroupBuff(GameObject caster, GraduationSkillSpec spec, IEnumerable<GameObject> allies)
        {
            StatModifierDefinition effect = GraduationSkillFactory.AllyEffect(spec);
            if (caster == null || effect == null || allies == null)
            {
                return 0;
            }

            var byId = new Dictionary<string, GameObject>();
            var candidates = new List<SupportCandidate>();
            foreach (GameObject ally in allies)
            {
                if (ally == null || ally == caster)
                {
                    continue;
                }

                string id = ally.GetInstanceID().ToString();
                byId[id] = ally;
                bool alive = !ally.TryGetComponent(out PlayerStats stats) || !stats.IsDead;
                candidates.Add(new SupportCandidate(id, ally.transform.position.x, ally.transform.position.z, alive));
            }

            Vector3 origin = caster.transform.position;
            int count = 0;
            foreach (string id in PartySupportRules.SelectAllies(origin.x, origin.z, spec.AreaRadius, candidates))
            {
                if (byId[id].TryGetComponent(out BuffController buffs))
                {
                    buffs.ApplyEffect(effect, GroupBuffSourcePrefix + spec.SkillId, caster);
                    count++;
                }
            }

            return count;
        }

        private void OnSkillCast(SkillCastEvent evt)
        {
            if (evt.Caster == null || evt.Skill == null || !evt.Caster.CompareTag(PlayerTag) || !TryActiveHero(out CharacterId hero))
            {
                return;
            }

            HandleCast(evt.Caster, hero, evt.Skill.SkillId, Clock());
            GraduationSkillSpec spec = ClassEvolutionCatalog.GetSkill(evt.Skill.SkillId);
            if (spec != null && spec.Kind == GraduationSkillKind.GroupBuff)
            {
                SpreadGroupBuff(evt.Caster, spec, GameObject.FindGameObjectsWithTag(PlayerTag));
            }
        }

        // ------------------------------------------------------------------ events

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null || !TryActiveHero(out CharacterId hero))
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            PlayerStats stats = evt.Killer != null ? evt.Killer.GetComponentInParent<PlayerStats>() : null;
            if (stats == null)
            {
                GameObject player = FindPlayer();
                stats = player != null ? player.GetComponent<PlayerStats>() : null;
            }

            BestiaryEntry entry = BestiaryCatalog.Get(DefeatedEnemyRegistry.GetId(enemy.Definition));
            RecordKill(hero, entry?.Region, stats != null ? stats.Level : 1);
        }

        private void OnCharacterSelected(CharacterSelectedEvent evt) => ApplyToPlayer(evt.CharacterId);

        private void OnCharacterRestored(CharacterSelectionRestoredEvent evt) => ApplyToPlayer(evt.CharacterId);

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            if (TryActiveHero(out CharacterId hero))
            {
                ApplyToPlayer(hero);
            }
        }

        private void ApplyToPlayer(CharacterId hero)
        {
            GameObject player = FindPlayer();
            if (player != null)
            {
                ApplyTo(player, hero, player.TryGetComponent(out PlayerStats stats) ? stats.Level : 1);
            }
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

        private static bool TryActiveHero(out CharacterId hero)
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
            var state = new ClassEvolutionSaveState();
            foreach (CharacterId hero in (CharacterId[])Enum.GetValues(typeof(CharacterId)))
            {
                GraduationRecord record = Record(hero);
                var copy = new GraduationRecord
                {
                    characterId = hero.ToString(),
                    tier = record.tier,
                    activeMissionTier = record.activeMissionTier,
                    missionKills = record.missionKills,
                    masterLevel = record.masterLevel,
                    masterExperience = record.masterExperience,
                };
                foreach (MasterNodeRank rank in record.masterNodes)
                {
                    copy.masterNodes.Add(new MasterNodeRank { nodeId = rank.nodeId, rank = rank.rank });
                }

                state.heroes.Add(copy);
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (state is not ClassEvolutionSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            if (saved.heroes == null)
            {
                return;
            }

            foreach (GraduationRecord entry in saved.heroes)
            {
                if (entry == null || !Enum.TryParse(entry.characterId, out CharacterId hero) || !Enum.IsDefined(typeof(CharacterId), hero))
                {
                    continue;
                }

                var record = new GraduationRecord
                {
                    characterId = hero.ToString(),
                    tier = entry.tier,
                    activeMissionTier = entry.activeMissionTier,
                    missionKills = entry.missionKills,
                    masterLevel = entry.masterLevel,
                    masterExperience = entry.masterExperience,
                    masterNodes = entry.masterNodes != null ? new List<MasterNodeRank>(entry.masterNodes) : new List<MasterNodeRank>(),
                };
                ClassEvolutionRules.Sanitize(hero, record);
                _records[hero] = record;
            }
        }
    }
}
