using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using UnityEngine;

namespace Rapadura.Gameplay.Hunting
{
    /// <summary>JSON-serializable snapshot of <see cref="HuntingProgress"/> (save key <see cref="HuntingProgress.Key"/>).</summary>
    [Serializable]
    public class HuntingSaveState
    {
        /// <summary>Day ("yyyy-MM-dd") the daily part belongs to; also a high-water mark against clock rollbacks.</summary>
        public string dayKey;
        public List<PreyOffer> preyOffers = new List<PreyOffer>();
        /// <summary>Index into <see cref="preyOffers"/> of today's pick, -1 = none.</summary>
        public int preyPick = -1;
        public int preyRerolls;
        public int tasksCompletedToday;

        /// <summary>The running task; JsonUtility never writes null, so an empty <see cref="HuntingTask.enemyId"/> means none.</summary>
        public HuntingTask task = new HuntingTask();
        public int huntPointsEarned;
        public int huntPointsAvailable;
        public int cadernoPointsTraded;
        public List<string> unlockedCharms = new List<string>();
        public List<string> equippedCharms = new List<string>();

        /// <summary>Task rewards not delivered yet (no live hero, or a full bag for the gold).</summary>
        public int pendingExperience;
        public int pendingGold;
    }

    /// <summary>Anything shown by the hunting UI changed (task, prey, charms, points, day).</summary>
    public readonly struct HuntingStateChangedEvent : IGameEvent
    {
    }

    /// <summary>A kill counted on the running "Tarefa de caça".</summary>
    public readonly struct HuntingTaskProgressEvent : IGameEvent
    {
        public readonly string EnemyId;
        public readonly int Progress;
        public readonly int Required;

        public HuntingTaskProgressEvent(string enemyId, int progress, int required)
        {
            EnemyId = enemyId;
            Progress = progress;
            Required = required;
        }
    }

    /// <summary>The running task was finished (rewards granted or queued).</summary>
    public readonly struct HuntingTaskCompletedEvent : IGameEvent
    {
        public readonly HuntingTask Task;

        public HuntingTaskCompletedEvent(HuntingTask task)
        {
            Task = task;
        }
    }

    /// <summary>
    /// The hunting features (Tibia-inspired, our names): "Tarefas de caça" from Patig, o Rastreador, "Presa do dia",
    /// "Bicho destaque do dia" and the "Amuletos do Caderno". Plain C# like <see cref="BestiaryProgress"/>: built by
    /// GameManager, registered with SaveManager under <see cref="Key"/> and with NewSessionReset. Rules live in
    /// <see cref="HuntingRules"/> / <see cref="HuntingCharmCatalog"/> (pure, tested); this class keeps the state and
    /// wires it into the game:
    /// <list type="bullet">
    /// <item><see cref="CombatTargetDiedEvent"/>: counts the kill on the task (rewards on the kill that completes it:
    /// bonus XP to the killer, gold into the bag through its grid fit, "Pontos de Caça"), heals with the
    /// Raminho de Arruda.</item>
    /// <item>Installs <see cref="EnemyRewardHooks"/> (prey/highlight XP and extra loot rolls) and
    /// <see cref="CombatModifierHooks"/> (prey/charm protection, Cornalina dodge, Nó de Caravaneiro stun) while initialized.</item>
    /// <item>Day rollover (<see cref="Refresh"/>, on init, load and every in-game hour, like <c>DailyQuestService</c>):
    /// new prey offers, pick and reroll cleared, the task cap reset. The running task stays.</item>
    /// <item>"Pontos do Caderno" are not stored: they are the Caderno's kill milestones (<see cref="BestiaryProgress"/>)
    /// plus the ones traded with Patig, minus the unlocked charms' cost, so an old save gets its points at once.</item>
    /// </list>
    /// A save written before this existed has no "Hunting" entry and loads with no task, no charms and today's offers.
    /// </summary>
    public class HuntingProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Hunting";
        public const string GoldItemId = "item_gold";
        private const string PlayerTag = "Player";

        private readonly IDailyClock _clock;
        private readonly Func<IReadOnlyList<HuntingCreature>> _creatures;
        private readonly Func<string, int> _killsOverride;
        private readonly Func<double> _random;

        private HuntingSaveState _state = new HuntingSaveState();
        private bool _subscribed;

        private Func<EnemyDefinition, float> _xpHook;
        private Func<EnemyDefinition, float> _lootHook;
        private Func<GameObject, GameObject, float> _damageHook;
        private Func<GameObject, GameObject, bool> _dodgeHook;
        private Func<GameObject, float> _stunHook;

        /// <param name="clock">Null = <see cref="SystemDailyClock"/> (online: the server's day, § "Caça").</param>
        /// <param name="creatures">Every Caderno page as <see cref="HuntingCreature"/> (default: <see cref="BestiaryCatalog"/>).</param>
        /// <param name="kills">Kills per page (default: the live <see cref="BestiaryProgress"/>).</param>
        /// <param name="random">Uniform [0, 1) source for dodge and extra loot (default: UnityEngine.Random).</param>
        public HuntingProgress(IDailyClock clock = null, Func<IReadOnlyList<HuntingCreature>> creatures = null,
            Func<string, int> kills = null, Func<double> random = null)
        {
            _clock = clock ?? SystemDailyClock.Instance;
            _creatures = creatures ?? CatalogCreatures;
            _killsOverride = kills;
            _random = random ?? (() => UnityEngine.Random.value);
        }

        public string SaveKey => Key;

        /// <summary>The live instance (ServiceLocator), or null outside a game session.</summary>
        public static HuntingProgress ResolveShared() => ServiceLocator.TryGet(out HuntingProgress progress) ? progress : null;

        // ------------------------------------------------------------------ read side

        public string DayKey => _state.dayKey;
        public IReadOnlyList<PreyOffer> PreyOffers => _state.preyOffers;
        public int PreyPickIndex => _state.preyPick;
        public PreyOffer ActivePrey => _state.preyPick >= 0 && _state.preyPick < _state.preyOffers.Count ? _state.preyOffers[_state.preyPick] : null;
        public int PreyRerollsToday => _state.preyRerolls;
        public HuntingTask ActiveTask => _state.task != null && !string.IsNullOrEmpty(_state.task.enemyId) ? _state.task : null;
        public int TasksCompletedToday => _state.tasksCompletedToday;
        public int HuntPointsEarned => _state.huntPointsEarned;
        public int HuntPointsAvailable => _state.huntPointsAvailable;
        public int HunterRank => HuntingRules.RankFor(_state.huntPointsEarned);
        public int CadernoPointsTraded => _state.cadernoPointsTraded;
        public IReadOnlyList<string> UnlockedCharms => _state.unlockedCharms;
        public IReadOnlyList<string> EquippedCharms => _state.equippedCharms;
        public int PendingExperience => _state.pendingExperience;
        public int PendingGold => _state.pendingGold;

        public IReadOnlyList<HuntingCreature> Creatures => _creatures() ?? Array.Empty<HuntingCreature>();

        public HuntingCreature? FindCreature(string id)
        {
            foreach (HuntingCreature creature in Creatures)
            {
                if (creature.Id == id)
                {
                    return creature;
                }
            }

            return null;
        }

        /// <summary>"Bicho destaque do dia" (null when there is no creature).</summary>
        public string BoostedId => HuntingRules.BoostedCreature(_state.dayKey, Creatures);

        public int GetKills(string enemyId)
        {
            if (_killsOverride != null)
            {
                return _killsOverride(enemyId);
            }

            return ServiceLocator.TryGet(out BestiaryProgress bestiary) && bestiary != null ? bestiary.GetKills(enemyId) : 0;
        }

        public bool IsUnlocked(string enemyId) => GetKills(enemyId) > 0;

        public List<HuntingCreature> EligibleCreatures() => HuntingRules.EligibleCreatures(Creatures, IsUnlocked);

        /// <summary>Creatures Patig offers tasks for (eligible ones, Caderno order, at most <see cref="HuntingRules.MaxTaskCreatures"/>).</summary>
        public List<HuntingCreature> TaskCreatures() => EligibleCreatures().Take(HuntingRules.MaxTaskCreatures).ToList();

        public int CadernoPointsFromMilestones => HuntingRules.TotalMilestonePoints(Creatures.Select(c => GetKills(c.Id)));

        public int CadernoPointsSpent => HuntingCharmCatalog.TotalCost(_state.unlockedCharms);

        public int CadernoPointsAvailable => HuntingRules.AvailableCadernoPoints(CadernoPointsFromMilestones, _state.cadernoPointsTraded, CadernoPointsSpent);

        public bool IsCharmUnlocked(string id) => _state.unlockedCharms.Contains(id);

        public bool IsCharmEquipped(string id) => _state.equippedCharms.Contains(id);

        // ------------------------------------------------------------------ lifecycle

        public void Initialize()
        {
            if (!_subscribed)
            {
                _subscribed = true;
                EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
                EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
                EventBus.Subscribe<TimeOfDayHourChangedEvent>(OnHourChanged);
            }

            InstallHooks();
            Refresh();
        }

        public void Shutdown()
        {
            if (_subscribed)
            {
                _subscribed = false;
                EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
                EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
                EventBus.Unsubscribe<TimeOfDayHourChangedEvent>(OnHourChanged);
            }

            RemoveHooks();
        }

        public void ResetForNewSession()
        {
            _state = new HuntingSaveState();
            Refresh();
            Changed();
        }

        /// <summary>Rolls the day over when the clock's date is later than the saved one; fills missing offers. True on a new day.</summary>
        public bool Refresh()
        {
            string today = DailyQuestService.ToDayKey(_clock.Now);
            bool newDay = HuntingRules.IsNewDay(_state.dayKey, today);
            if (newDay)
            {
                _state.dayKey = today;
                _state.preyOffers.Clear();
                _state.preyPick = -1;
                _state.preyRerolls = 0;
                _state.tasksCompletedToday = 0;
            }

            if (_state.preyOffers.Count == 0)
            {
                _state.preyOffers.AddRange(HuntingRules.PreyOffers(_state.dayKey, _state.preyRerolls, EligibleCreatures(), BoostedId));
                if (_state.preyPick >= _state.preyOffers.Count)
                {
                    _state.preyPick = -1;
                }
            }

            if (newDay)
            {
                Changed();
            }

            return newDay;
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            if (!Refresh())
            {
                Changed();
            }
        }

        private void OnHourChanged(TimeOfDayHourChangedEvent evt) => Refresh();

        private static void Changed() => EventBus.Publish(new HuntingStateChangedEvent());

        // ------------------------------------------------------------------ tasks

        public bool CanTakeTask => HuntingRules.CanTakeTask(ActiveTask, _state.tasksCompletedToday);

        /// <summary>Takes a task against <paramref name="enemyId"/> (must be one of <see cref="TaskCreatures"/>).</summary>
        public bool TakeTask(string enemyId, HuntingTaskTier tier)
        {
            Refresh();
            if (!CanTakeTask)
            {
                return false;
            }

            HuntingCreature? creature = TaskCreatures().Cast<HuntingCreature?>().FirstOrDefault(c => c.Value.Id == enemyId);
            HuntingTask task = creature.HasValue ? HuntingRules.CreateTask(creature.Value, tier) : null;
            if (task == null)
            {
                return false;
            }

            _state.task = task;
            Changed();
            return true;
        }

        /// <summary>Drops the running task: no reward, progress lost (a new one can be taken right away).</summary>
        public bool CancelTask()
        {
            if (ActiveTask == null)
            {
                return false;
            }

            _state.task = new HuntingTask();
            Changed();
            return true;
        }

        /// <summary>Counts a kill of <paramref name="enemyId"/>; rewards go to <paramref name="stats"/>/<paramref name="inventory"/> (or wait for them).</summary>
        public bool RecordKill(string enemyId, PlayerStats stats = null, InventoryManager inventory = null)
        {
            HuntingTask task = ActiveTask;
            if (task == null || task.enemyId != enemyId)
            {
                DeliverPending(stats, inventory);
                return false;
            }

            bool completed = HuntingRules.RecordKill(task, enemyId);
            EventBus.Publish(new HuntingTaskProgressEvent(task.enemyId, task.progress, task.required));
            if (completed)
            {
                Complete(task);
            }

            DeliverPending(stats, inventory);
            Changed();
            return completed;
        }

        private void Complete(HuntingTask task)
        {
            _state.task = new HuntingTask();
            _state.tasksCompletedToday++;
            _state.huntPointsEarned += task.rewardHuntPoints;
            _state.huntPointsAvailable += task.rewardHuntPoints;
            _state.pendingExperience += task.rewardExperience;
            _state.pendingGold += task.rewardGold;
            EventBus.Publish(new HuntingTaskCompletedEvent(task));
        }

        /// <summary>Hands queued task rewards to a live hero: XP to <paramref name="stats"/>, gold that fits (grid) into <paramref name="inventory"/>.</summary>
        public void DeliverPending(PlayerStats stats, InventoryManager inventory)
        {
            if (stats != null && _state.pendingExperience > 0)
            {
                int xp = _state.pendingExperience;
                _state.pendingExperience = 0;
                stats.AddExperience(xp);
            }

            if (inventory != null && _state.pendingGold > 0)
            {
                ItemDefinition gold = ItemDatabase.GetById(GoldItemId);
                if (gold != null)
                {
                    int fits = inventory.GetFittableAmount(gold, _state.pendingGold);
                    if (fits > 0)
                    {
                        int leftover = inventory.AddItem(gold, fits);
                        _state.pendingGold -= fits - leftover;
                    }
                }
            }
        }

        // ------------------------------------------------------------------ prey

        public bool PickPrey(int index)
        {
            Refresh();
            if (index < 0 || index >= _state.preyOffers.Count || _state.preyPick == index)
            {
                return false;
            }

            _state.preyPick = index;
            Changed();
            return true;
        }

        public bool CanRerollPrey(int gold) => HuntingRules.CanRerollPrey(_state.preyRerolls, gold);

        /// <summary>
        /// New prey offers for today (the pick is cleared). <paramref name="payGold"/> takes <see cref="HuntingRules.PreyRerollGold"/>
        /// from the bag and returns false when it couldn't; nothing changes then.
        /// </summary>
        public bool RerollPrey(int goldHeld, Func<int, bool> payGold)
        {
            Refresh();
            if (!CanRerollPrey(goldHeld) || payGold == null || !payGold(HuntingRules.PreyRerollGold))
            {
                return false;
            }

            _state.preyRerolls++;
            _state.preyPick = -1;
            _state.preyOffers.Clear();
            _state.preyOffers.AddRange(HuntingRules.PreyOffers(_state.dayKey, _state.preyRerolls, EligibleCreatures(), BoostedId));
            Changed();
            return true;
        }

        // ------------------------------------------------------------------ charms / points

        public bool UnlockCharm(string id)
        {
            if (!HuntingCharmCatalog.CanUnlock(id, _state.unlockedCharms, CadernoPointsAvailable))
            {
                return false;
            }

            _state.unlockedCharms.Add(id);
            if (_state.equippedCharms.Count < HuntingCharmCatalog.MaxEquipped)
            {
                _state.equippedCharms.Add(id);
            }

            Changed();
            return true;
        }

        public bool EquipCharm(string id)
        {
            if (!HuntingCharmCatalog.CanEquip(id, _state.unlockedCharms, _state.equippedCharms))
            {
                return false;
            }

            _state.equippedCharms.Add(id);
            Changed();
            return true;
        }

        public bool UnequipCharm(string id)
        {
            if (!_state.equippedCharms.Remove(id))
            {
                return false;
            }

            Changed();
            return true;
        }

        public bool CanTradeHuntPoints => _state.huntPointsAvailable >= HuntingRules.HuntPointsPerCadernoPoint;

        /// <summary>Patig's trade: <see cref="HuntingRules.HuntPointsPerCadernoPoint"/> "Pontos de Caça" → 1 "Ponto do Caderno".</summary>
        public bool TradeHuntPoints()
        {
            if (!CanTradeHuntPoints)
            {
                return false;
            }

            _state.huntPointsAvailable -= HuntingRules.HuntPointsPerCadernoPoint;
            _state.cadernoPointsTraded++;
            Changed();
            return true;
        }

        // ------------------------------------------------------------------ bonuses (also what the hooks read)

        public static HuntingCreature CreatureOf(EnemyDefinition definition)
        {
            return definition == null
                ? default
                : new HuntingCreature(DefeatedEnemyRegistry.GetId(definition), definition.MotionArchetype, definition.IsBoss, 0, definition.ExperienceReward);
        }

        public float ExperienceMultiplier(HuntingCreature creature) => HuntingRules.ExperienceMultiplier(creature, ActivePrey, BoostedId);

        public float ExtraLootChance(HuntingCreature creature) => HuntingRules.ExtraLootChance(creature, ActivePrey, BoostedId);

        public float DamageTakenMultiplier(HuntingCreature creature) => HuntingRules.DamageTakenMultiplier(creature, ActivePrey, _state.equippedCharms);

        private void InstallHooks()
        {
            _xpHook = definition => ExperienceMultiplier(CreatureOf(definition));
            _lootHook = definition => ExtraLootChance(CreatureOf(definition));
            _damageHook = DamageHook;
            _dodgeHook = DodgeHook;
            _stunHook = StunHook;
            EnemyRewardHooks.ExperienceMultiplier = _xpHook;
            EnemyRewardHooks.ExtraLootChance = _lootHook;
            CombatModifierHooks.DamageTakenMultiplier = _damageHook;
            CombatModifierHooks.DodgeCheck = _dodgeHook;
            CombatModifierHooks.StunReceivedMultiplier = _stunHook;
        }

        private void RemoveHooks()
        {
            // Only our own: another system may have replaced one meanwhile.
            if (EnemyRewardHooks.ExperienceMultiplier == _xpHook) EnemyRewardHooks.ExperienceMultiplier = null;
            if (EnemyRewardHooks.ExtraLootChance == _lootHook) EnemyRewardHooks.ExtraLootChance = null;
            if (CombatModifierHooks.DamageTakenMultiplier == _damageHook) CombatModifierHooks.DamageTakenMultiplier = null;
            if (CombatModifierHooks.DodgeCheck == _dodgeHook) CombatModifierHooks.DodgeCheck = null;
            if (CombatModifierHooks.StunReceivedMultiplier == _stunHook) CombatModifierHooks.StunReceivedMultiplier = null;
        }

        private float DamageHook(GameObject target, GameObject source)
        {
            if (source == null || target.GetComponent<PlayerStats>() == null)
            {
                return 1f;
            }

            EnemyController enemy = source.GetComponentInParent<EnemyController>();
            return enemy != null && enemy.Definition != null ? DamageTakenMultiplier(CreatureOf(enemy.Definition)) : 1f;
        }

        private bool DodgeHook(GameObject victim, GameObject attacker)
        {
            EnemyController enemy = attacker.GetComponentInParent<EnemyController>();
            if (enemy == null || victim.GetComponent<PlayerStats>() == null)
            {
                return false;
            }

            bool dodged = HuntingCharmCatalog.RollDodge(HuntingCharmCatalog.DodgeChanceAgainst(_state.equippedCharms, enemy.IsBoss), _random());
            if (dodged && Application.isPlaying)
            {
                CombatFxManager.SpawnLootText(victim.transform.position + Vector3.up * 2f, HuntingTexts.Get(HuntingTexts.Live, HuntingTexts.DodgeFloat));
            }

            return dodged;
        }

        private float StunHook(GameObject target)
        {
            EnemyController enemy = target.GetComponent<EnemyController>();
            return enemy != null ? HuntingCharmCatalog.StunMultiplier(_state.equippedCharms, enemy.IsBoss) : 1f;
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            GameObject hero = evt.Killer;
            PlayerStats stats = hero != null ? hero.GetComponentInParent<PlayerStats>() : null;
            if (stats == null)
            {
                // A kill with no hero source (damage over time): this is a single-hero game.
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                stats = player != null ? player.GetComponent<PlayerStats>() : null;
            }

            InventoryManager inventory = stats != null ? stats.GetComponent<InventoryManager>() : null;

            if (stats != null && !stats.IsDead)
            {
                float heal = HuntingCharmCatalog.HealOnKill(_state.equippedCharms, enemy.IsBoss, stats.MaxHealth);
                if (heal > 0f)
                {
                    stats.Heal(heal);
                }
            }

            RecordKill(DefeatedEnemyRegistry.GetId(enemy.Definition), stats, inventory);
        }

        // ------------------------------------------------------------------ default catalog

        private static IReadOnlyList<HuntingCreature> CatalogCreatures()
        {
            IReadOnlyList<BestiaryEntry> entries = BestiaryCatalog.Entries;
            var creatures = new List<HuntingCreature>(entries.Count);
            foreach (BestiaryEntry entry in entries)
            {
                creatures.Add(new HuntingCreature(entry.Id, entry.Archetype, entry.IsBoss,
                    entry.Region.HasValue ? (int)entry.Region.Value : int.MaxValue,
                    entry.Definition != null ? entry.Definition.ExperienceReward : 0));
            }

            return creatures;
        }

        // ------------------------------------------------------------------ save

        public object CaptureState()
        {
            return new HuntingSaveState
            {
                dayKey = _state.dayKey,
                preyOffers = _state.preyOffers.Select(o => new PreyOffer(o.enemyId, o.bonus)).ToList(),
                preyPick = _state.preyPick,
                preyRerolls = _state.preyRerolls,
                tasksCompletedToday = _state.tasksCompletedToday,
                task = ActiveTask != null ? Copy(ActiveTask) : new HuntingTask(),
                huntPointsEarned = _state.huntPointsEarned,
                huntPointsAvailable = _state.huntPointsAvailable,
                cadernoPointsTraded = _state.cadernoPointsTraded,
                unlockedCharms = _state.unlockedCharms.ToList(),
                equippedCharms = _state.equippedCharms.ToList(),
                pendingExperience = _state.pendingExperience,
                pendingGold = _state.pendingGold,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not HuntingSaveState saved)
            {
                return;
            }

            _state = Sanitize(saved);
            Refresh();
            Changed();
        }

        /// <summary>A hand-edited or older save never breaks the rules: unknown charms dropped, counts clamped, bad offers removed.</summary>
        public static HuntingSaveState Sanitize(HuntingSaveState saved)
        {
            var clean = new HuntingSaveState
            {
                dayKey = DailyQuestService.DaySeed(saved.dayKey) != 0 ? saved.dayKey : null,
                preyRerolls = Mathf.Clamp(saved.preyRerolls, 0, HuntingRules.MaxPreyRerollsPerDay),
                tasksCompletedToday = Mathf.Clamp(saved.tasksCompletedToday, 0, HuntingRules.MaxTasksPerDay),
                huntPointsEarned = Math.Max(0, saved.huntPointsEarned),
                huntPointsAvailable = Mathf.Clamp(saved.huntPointsAvailable, 0, Math.Max(0, saved.huntPointsEarned)),
                cadernoPointsTraded = Math.Max(0, saved.cadernoPointsTraded),
                pendingExperience = Math.Max(0, saved.pendingExperience),
                pendingGold = Math.Max(0, saved.pendingGold),
            };

            if (saved.preyOffers != null)
            {
                clean.preyOffers.AddRange(saved.preyOffers.Where(o => o != null && !string.IsNullOrEmpty(o.enemyId)
                    && Enum.IsDefined(typeof(PreyBonusKind), o.bonus)).Take(HuntingRules.PreyOfferCount));
            }

            clean.preyPick = saved.preyPick >= 0 && saved.preyPick < clean.preyOffers.Count ? saved.preyPick : -1;

            HuntingTask task = saved.task;
            if (task != null && !string.IsNullOrEmpty(task.enemyId) && task.required > 0 && Enum.IsDefined(typeof(HuntingTaskTier), task.tier))
            {
                HuntingTask copy = Copy(task);
                copy.progress = Mathf.Clamp(copy.progress, 0, copy.required);
                clean.task = copy;
            }

            if (saved.unlockedCharms != null)
            {
                clean.unlockedCharms.AddRange(saved.unlockedCharms.Where(HuntingCharmCatalog.Contains).Distinct());
            }

            if (saved.equippedCharms != null)
            {
                clean.equippedCharms.AddRange(saved.equippedCharms.Where(id => clean.unlockedCharms.Contains(id)).Distinct().Take(HuntingCharmCatalog.MaxEquipped));
            }

            return clean;
        }

        private static HuntingTask Copy(HuntingTask task) => new HuntingTask
        {
            enemyId = task.enemyId,
            tier = task.tier,
            required = task.required,
            progress = task.progress,
            rewardExperience = Math.Max(0, task.rewardExperience),
            rewardGold = Math.Max(0, task.rewardGold),
            rewardHuntPoints = Math.Max(0, task.rewardHuntPoints),
        };
    }
}
