using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>One mount the cousins own (Docs/Mounts.md §12.3). Several of the same species can exist (own <see cref="guid"/>).</summary>
    [Serializable]
    public class OwnedMount
    {
        public string guid;
        public string mountId;
        public string nickname = string.Empty;
        public int level = 1;
        public int xp;

        /// <summary>0..500 (100 per heart, M2).</summary>
        public int affection;

        /// <summary>0..100 (M2).</summary>
        public float satiety = 100f;

        /// <summary>Health left; negative = full (never saved below 0 otherwise).</summary>
        public float currentHealth = -1f;

        /// <summary>Offline: encounters the mount still rests after getting tired (0 = ready). Online would be a UTC time.</summary>
        public int tiredUntilEncounter;

        public string saddleId = string.Empty;
        public string harnessId = string.Empty;
        public string blanketId = string.Empty;
        public string bellId = string.Empty;
        public string trailId = string.Empty;

        /// <summary>Stable slot at the house (-1 = kept at the town's Haras).</summary>
        public int stableSlot = -1;

        /// <summary>shop / drop / quest / achievement / event / secret / ribbon.</summary>
        public string source = string.Empty;
        public long acquiredUtc;
        public bool bound;
        public int hatchEncountersLeft;

        /// <summary>Carry-over of the favourite's ×1.25 affection (M2).</summary>
        public float affectionFraction;

        /// <summary>Metres ridden in total (1 XP per 20 m, +1 affection per km).</summary>
        public float metersRidden;

        public bool IsTired => MountRules.IsTired(tiredUntilEncounter);
        public int Hearts => MountCareRules.Hearts(affection);
        public bool IsHungry => MountCareRules.IsHungry(satiety);
    }

    /// <summary>JSON snapshot of <see cref="MountCollection"/> (parallel lists: JsonUtility has no dictionaries).</summary>
    [Serializable]
    public class MountCollectionSaveState
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string activeMountGuid = string.Empty;
        public int harasExtraSlots;
        public List<OwnedMount> mounts = new List<OwnedMount>();

        /// <summary>"Fitas do Laço" per boss enemy id (M3): parallel to <see cref="bossRibbonCounts"/>.</summary>
        public List<string> bossRibbonIds = new List<string>();
        public List<int> bossRibbonCounts = new List<int>();

        /// <summary>Mounts already seen (silhouette in the Caderno, M2).</summary>
        public List<string> codexSeen = new List<string>();

        /// <summary>Saddles and blankets bought at the Selaria (M2), shared by every mount.</summary>
        public List<string> ownedCosmetics = new List<string>();

        /// <summary>Day index of the last Bebedouro affection (one a day).</summary>
        public long lastWaterDay = -1;
    }

    /// <summary>A mount joined the collection (shop whistle, "Primeira Sela"...).</summary>
    public readonly struct MountGrantedEvent : IGameEvent
    {
        public readonly string MountId;
        public readonly string Guid;
        public readonly string Source;

        public MountGrantedEvent(string mountId, string guid, string source)
        {
            MountId = mountId;
            Guid = guid;
            Source = source;
        }
    }

    /// <summary>The active mount ran out of health and went home to rest.</summary>
    public readonly struct MountTiredEvent : IGameEvent
    {
        public readonly string MountId;

        public MountTiredEvent(string mountId)
        {
            MountId = mountId;
        }
    }

    /// <summary>A mount levelled up (M2).</summary>
    public readonly struct MountLeveledEvent : IGameEvent
    {
        public readonly string MountId;
        public readonly int Level;

        public MountLeveledEvent(string mountId, int level)
        {
            MountId = mountId;
            Level = level;
        }
    }

    /// <summary>Anything in the collection changed (active mount, health, rest): the HUD refreshes.</summary>
    public readonly struct MountCollectionChangedEvent : IGameEvent
    {
    }

    /// <summary>
    /// The cousins' mounts, <b>one collection per save/account, shared by the five cousins</b> (Decisão 5 of
    /// Docs/Mounts.md, offline proposal), like the Baú da Vó Zefa. A GameManager-built manager registered with
    /// SaveManager under <see cref="Key"/> and with NewSessionReset; a save written before mounts existed has no entry
    /// and loads with no mount. Also:
    /// <list type="bullet">
    /// <item>buying an "Apito de …" (<see cref="MountCatalog.FindByWhistle"/>) at the Haras redeems it at once: the
    /// whistle leaves the bag and the mount joins the collection (and becomes active when there was none);</item>
    /// <item>the end of every encounter rests the mounts: a tired one counts down <see cref="MountRules.TiredEncounters"/>
    /// fights and comes back at full health; the others are healed (they waited at the edge of the arena).</item>
    /// </list>
    /// </summary>
    public class MountCollection : IManager, ISaveable, IResettable
    {
        public const string Key = "Mounts";
        public const string SourceShop = "shop";
        public const string SourceQuest = "quest";
        public const string SourceDrop = "drop";
        public const string SourceAchievement = "achievement";
        public const string SourceSecret = "secret";
        public const string SourceEvent = "event";
        public const string SourceRibbon = "ribbon";

        /// <summary>Random source of the mount drops (§3.6). Offline the game owns it; online the server injects its own and the client never rolls.</summary>
        public System.Random DropRandom { get; set; } = new System.Random();

        /// <summary>Whether it is night now (the Lobo-Guará doubles and the Onça shows up at night). Tests inject one.</summary>
        public Func<bool> IsNight { get; set; } = DefaultIsNight;

        private readonly List<OwnedMount> _mounts = new List<OwnedMount>();
        private readonly Dictionary<string, int> _ribbons = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _codexSeen = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _cosmetics = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _lastCuddle = new Dictionary<string, float>(StringComparer.Ordinal);
        private long _lastWaterDay = -1;
        private string _activeGuid = string.Empty;
        private int _harasExtraSlots;
        private bool _subscribed;

        private static MountCollection _standalone;

        /// <summary>Instance ids (tests inject a deterministic one).</summary>
        public Func<string> GuidFactory { get; set; } = () => System.Guid.NewGuid().ToString("N");

        /// <summary>UTC clock for <see cref="OwnedMount.acquiredUtc"/> (unix seconds).</summary>
        public Func<long> UtcNow { get; set; } = () => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public string SaveKey => Key;

        public IReadOnlyList<OwnedMount> Mounts => _mounts;
        public int Count => _mounts.Count;
        public int HarasExtraSlots => _harasExtraSlots;
        public IReadOnlyCollection<string> CodexSeen => _codexSeen;
        public IReadOnlyCollection<string> OwnedCosmetics => _cosmetics;

        /// <summary>Day clock for the Bebedouro (tests inject one).</summary>
        public Func<long> DayIndex { get; set; } = () => DateTime.Now.Date.Ticks / TimeSpan.TicksPerDay;

        public OwnedMount Active => Find(_activeGuid);

        /// <summary>The definition of the active mount, or null (none owned / unknown species).</summary>
        public MountDefinition ActiveDefinition => Active != null ? MountCatalog.Get(Active.mountId) : null;

        /// <summary>The GameManager's collection, or a process-wide unsaved one when a scene is played on its own.</summary>
        public static MountCollection ResolveShared()
        {
            if (ServiceLocator.TryGet(out MountCollection collection) && collection != null)
            {
                return collection;
            }

            if (_standalone == null)
            {
                _standalone = new MountCollection();
                _standalone.Initialize();
            }

            return _standalone;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _standalone?.Shutdown();
            _standalone = null;
        }

        // ------------------------------------------------------------------
        // IManager / IResettable
        // ------------------------------------------------------------------

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<ItemPurchasedEvent>(OnItemPurchased);
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Subscribe<Rapadura.Gameplay.Enemies.EnemyDiedEvent>(OnEnemyDied);
            EventBus.Subscribe<Rapadura.Gameplay.Quests.QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Subscribe<Rapadura.Gameplay.Achievements.AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Subscribe<Rapadura.Gameplay.Secrets.SecretClaimedEvent>(OnSecretClaimed);

            if (ServiceLocator.TryGet(out Rapadura.Core.Localization.LocalizationManager localization))
            {
                MountTexts.EnsureRegistered(localization);
            }

            // "Primeira Sela" lives on the GameManager's quest log (its definition must exist before a save restores it).
            if (ServiceLocator.TryGet(out Rapadura.Gameplay.Quests.QuestManager quests))
            {
                FirstSaddleQuest.Install(quests, this);
            }
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<ItemPurchasedEvent>(OnItemPurchased);
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Unsubscribe<Rapadura.Gameplay.Enemies.EnemyDiedEvent>(OnEnemyDied);
            EventBus.Unsubscribe<Rapadura.Gameplay.Quests.QuestCompletedEvent>(OnQuestCompleted);
            EventBus.Unsubscribe<Rapadura.Gameplay.Achievements.AchievementUnlockedEvent>(OnAchievementUnlocked);
            EventBus.Unsubscribe<Rapadura.Gameplay.Secrets.SecretClaimedEvent>(OnSecretClaimed);
            FirstSaddleQuest.Uninstall();
        }

        public void ResetForNewSession()
        {
            _mounts.Clear();
            _ribbons.Clear();
            _codexSeen.Clear();
            _cosmetics.Clear();
            _lastCuddle.Clear();
            _lastWaterDay = -1;
            _activeGuid = string.Empty;
            _harasExtraSlots = 0;
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public OwnedMount Find(string guid)
        {
            return string.IsNullOrEmpty(guid) ? null : _mounts.FirstOrDefault(m => m.guid == guid);
        }

        public bool Owns(string mountId) => _mounts.Any(m => m.mountId == mountId);

        public int RibbonsOf(string enemyId) => enemyId != null && _ribbons.TryGetValue(enemyId, out int n) ? n : 0;

        /// <summary>Max health of <paramref name="mount"/> at its level.</summary>
        public static float MaxHealthOf(OwnedMount mount)
        {
            MountDefinition definition = mount != null ? MountCatalog.Get(mount.mountId) : null;
            return definition != null ? MountRules.MaxHealthAt(definition.BaseHealth, mount.level) * MountCareRules.HealthMultiplier(mount.Hearts) : 100f;
        }

        public static float MaxStaminaOf(OwnedMount mount)
        {
            MountDefinition definition = mount != null ? MountCatalog.Get(mount.mountId) : null;
            return definition != null ? MountRules.MaxStaminaAt(definition.BaseStamina, mount.level) : 100f;
        }

        /// <summary>Current health of <paramref name="mount"/> (full when never hurt).</summary>
        public static float HealthOf(OwnedMount mount)
        {
            if (mount == null)
            {
                return 0f;
            }

            float max = MaxHealthOf(mount);
            return mount.currentHealth < 0f ? max : Mathf.Min(mount.currentHealth, max);
        }

        // ------------------------------------------------------------------
        // Changes
        // ------------------------------------------------------------------

        /// <summary>Adds a mount of <paramref name="mountId"/>; it becomes the active one when there was none. Null for an unknown species.</summary>
        public OwnedMount Grant(string mountId, string source)
        {
            if (MountCatalog.FindSpec(mountId) == null && MountCatalog.Get(mountId) == null)
            {
                return null;
            }

            var mount = new OwnedMount
            {
                guid = GuidFactory != null ? GuidFactory() : System.Guid.NewGuid().ToString("N"),
                mountId = mountId,
                source = source ?? string.Empty,
                acquiredUtc = UtcNow != null ? UtcNow() : 0L,
            };
            _mounts.Add(mount);
            _codexSeen.Add(mountId);
            if (Active == null)
            {
                _activeGuid = mount.guid;
            }

            EventBus.Publish(new MountGrantedEvent(mountId, mount.guid, mount.source));
            EventBus.Publish(new MountCollectionChangedEvent());
            return mount;
        }

        public bool SetActive(string guid)
        {
            if (Find(guid) == null || guid == _activeGuid)
            {
                return false;
            }

            _activeGuid = guid;
            EventBus.Publish(new MountCollectionChangedEvent());
            return true;
        }

        /// <summary>Writes the health of <paramref name="mount"/> (clamped); reaching 0 makes it tired. True when it just got tired.</summary>
        public bool SetHealth(OwnedMount mount, float health)
        {
            if (mount == null)
            {
                return false;
            }

            mount.currentHealth = Mathf.Clamp(health, 0f, MaxHealthOf(mount));
            if (!mount.IsTired && MountRules.BecomesTired(mount.currentHealth))
            {
                mount.tiredUntilEncounter = MountRules.TiredEncounters;
                EventBus.Publish(new MountTiredEvent(mount.mountId));
                EventBus.Publish(new MountCollectionChangedEvent());
                return true;
            }

            return false;
        }

        /// <summary>One encounter is over: tired mounts rest one fight, the others get their health back.</summary>
        public void RestAfterEncounter()
        {
            foreach (OwnedMount mount in _mounts)
            {
                if (mount.IsTired)
                {
                    mount.tiredUntilEncounter = MountRules.RestAfterEncounter(mount.tiredUntilEncounter);
                    if (!mount.IsTired)
                    {
                        mount.currentHealth = -1f;
                    }
                }
                else
                {
                    mount.currentHealth = -1f;
                }
            }

            EventBus.Publish(new MountCollectionChangedEvent());
        }

        // ------------------------------------------------------------------
        // Care (M2)
        // ------------------------------------------------------------------

        /// <summary>Adds XP; publishes <see cref="MountLeveledEvent"/> per level gained. Returns levels gained.</summary>
        public int AddXp(OwnedMount mount, int amount)
        {
            if (mount == null || amount <= 0)
            {
                return 0;
            }

            int gained = MountCareRules.AddXp(ref mount.level, ref mount.xp, amount);
            if (gained > 0)
            {
                EventBus.Publish(new MountLeveledEvent(mount.mountId, mount.level));
            }

            EventBus.Publish(new MountCollectionChangedEvent());
            return gained;
        }

        public void AddAffection(OwnedMount mount, int points, bool isFavorite)
        {
            if (mount == null || points <= 0)
            {
                return;
            }

            mount.affection = MountCareRules.AddAffection(mount.affection, ref mount.affectionFraction, points, isFavorite);
            EventBus.Publish(new MountCollectionChangedEvent());
        }

        /// <summary>Counts metres ridden: 1 XP per 20 m, +1 affection per km.</summary>
        public void AddRideDistance(OwnedMount mount, float meters, bool isFavorite)
        {
            if (mount == null || meters <= 0f)
            {
                return;
            }

            float before = mount.metersRidden;
            mount.metersRidden += meters;
            int xp = Mathf.FloorToInt(mount.metersRidden / MountCareRules.MetersPerXp) - Mathf.FloorToInt(before / MountCareRules.MetersPerXp);
            int km = Mathf.FloorToInt(mount.metersRidden / MountCareRules.MetersPerAffection) - Mathf.FloorToInt(before / MountCareRules.MetersPerAffection);
            if (xp > 0)
            {
                AddXp(mount, xp);
            }

            if (km > 0)
            {
                AddAffection(mount, km, isFavorite);
            }
        }

        /// <summary>Satiety falls while mounted; true the moment the mount becomes hungry.</summary>
        public bool DecaySatiety(OwnedMount mount, float seconds)
        {
            if (mount == null || seconds <= 0f)
            {
                return false;
            }

            bool wasHungry = mount.IsHungry;
            mount.satiety = MountCareRules.DecaySatiety(mount.satiety, seconds);
            return !wasHungry && mount.IsHungry;
        }

        /// <summary>Feeds an already-paid <paramref name="food"/>; false when the diet refuses it.</summary>
        public bool Feed(OwnedMount mount, MountFoodSpec food, bool isFavorite)
        {
            MountDefinition definition = mount != null ? MountCatalog.Get(mount.mountId) : null;
            if (definition == null || !MountCareRules.CanEat(definition.Diet, food))
            {
                return false;
            }

            mount.satiety = Mathf.Clamp(mount.satiety + food.Satiety, 0f, MountCareRules.MaxSatiety);
            AddAffection(mount, food.Affection, isFavorite);
            AddXp(mount, food.Xp);
            EventBus.Publish(new MountCollectionChangedEvent());
            return true;
        }

        /// <summary>Petting: +1 affection, once every <see cref="MountCareRules.CuddleCooldownSeconds"/> per mount.</summary>
        public bool TryCuddle(OwnedMount mount, float now, bool isFavorite)
        {
            if (mount == null)
            {
                return false;
            }

            float last = _lastCuddle.TryGetValue(mount.guid, out float t) ? t : float.NegativeInfinity;
            if (!MountCareRules.CanCuddle(last, now))
            {
                return false;
            }

            _lastCuddle[mount.guid] = now;
            AddAffection(mount, MountCareRules.CuddleAffection, isFavorite);
            return true;
        }

        public bool OwnsCosmetic(string cosmeticId) => cosmeticId != null && _cosmetics.Contains(cosmeticId);

        public bool GrantCosmetic(string cosmeticId)
        {
            if (MountCosmetics.Get(cosmeticId) == null || !_cosmetics.Add(cosmeticId))
            {
                return false;
            }

            EventBus.Publish(new MountCollectionChangedEvent());
            return true;
        }

        /// <summary>Puts an owned saddle/blanket on <paramref name="mount"/> (null/empty id = plain).</summary>
        public bool Equip(OwnedMount mount, MountCosmeticSlot slot, string cosmeticId)
        {
            if (mount == null)
            {
                return false;
            }

            MountCosmeticSpec spec = MountCosmetics.Get(cosmeticId);
            if (!string.IsNullOrEmpty(cosmeticId) && (spec == null || spec.Slot != slot || !OwnsCosmetic(cosmeticId)))
            {
                return false;
            }

            if (slot == MountCosmeticSlot.Saddle)
            {
                mount.saddleId = cosmeticId ?? string.Empty;
            }
            else
            {
                mount.blanketId = cosmeticId ?? string.Empty;
            }

            EventBus.Publish(new MountCollectionChangedEvent());
            return true;
        }

        /// <summary>The mounts in the house's stalls (the first ones after the active mount, up to the stall count).</summary>
        public List<OwnedMount> InStalls(int stalls)
        {
            return _mounts.Where(m => m.guid != _activeGuid).Take(Math.Max(0, stalls)).ToList();
        }

        /// <summary>The stable's perks after a fight (§7): cocho/manjedoura feed the stalls, the bebedouro once a day.</summary>
        public void ApplyStable(MountStableInfo stable)
        {
            if (!stable.HasHouse || stable.Stalls <= 0)
            {
                return;
            }

            List<OwnedMount> stalled = InStalls(stable.Stalls);
            if (stable.FeedsPerFight)
            {
                foreach (OwnedMount mount in stalled)
                {
                    mount.satiety = Mathf.Min(MountCareRules.MaxSatiety, mount.satiety + MountCareRules.StableFeedPerFight);
                }
            }

            long day = DayIndex != null ? DayIndex() : 0L;
            if (stable.WatersDaily && day != _lastWaterDay)
            {
                _lastWaterDay = day;
                foreach (OwnedMount mount in stalled)
                {
                    mount.affection = MountCareRules.AddAffection(mount.affection, ref mount.affectionFraction, 1, false);
                }
            }
        }

        /// <summary>Redeems a whistle: the mount joins the collection. Returns the new mount, or null when <paramref name="itemId"/> is no whistle.</summary>
        public OwnedMount RedeemWhistle(string itemId, string source = SourceShop)
        {
            MountSpec spec = MountCatalog.FindByWhistle(itemId);
            return spec != null ? Grant(spec.MountId, source) : null;
        }

        private void OnItemPurchased(ItemPurchasedEvent evt)
        {
            if (!MountCatalog.IsWhistle(evt.ItemId))
            {
                return;
            }

            for (int i = 0; i < Math.Max(1, evt.Quantity); i++)
            {
                RedeemWhistle(evt.ItemId);
            }

            // The whistle itself only carries the purchase: take it back out of the buyer's bag.
            if (ServiceLocator.TryGet(out ShopManager shops) && shops?.BuyerInventory != null)
            {
                shops.BuyerInventory.RemoveById(evt.ItemId, Math.Max(1, evt.Quantity));
            }
        }

        // ------------------------------------------------------------------
        // M3 sources: drops, Fitas do Laco, missions/achievements/secrets, Festa Junina
        // ------------------------------------------------------------------

        /// <summary>
        /// One death of <paramref name="enemyId"/> (an <c>EnemyDefinition</c> asset name): rolls its mount drops
        /// (<see cref="MountDropRules.Roll"/> with <see cref="DropRandom"/>; a drop joins the collection at once, source
        /// <see cref="SourceDrop"/>) and, for a region boss, adds one Fita do Laço. Returns the mounts that dropped.
        /// </summary>
        public List<OwnedMount> ResolveEnemyDeath(string enemyId, bool isBoss, bool isNight)
        {
            var granted = new List<OwnedMount>();
            if (string.IsNullOrEmpty(enemyId))
            {
                return granted;
            }

            foreach (string mountId in MountDropRules.Roll(enemyId, isNight, DropRandom))
            {
                OwnedMount mount = Grant(mountId, SourceDrop);
                if (mount != null)
                {
                    granted.Add(mount);
                }
            }

            if (isBoss && MountDropRules.GivesRibbons(enemyId))
            {
                AddRibbons(enemyId, MountDropRules.RibbonsPerBossVictory);
            }

            return granted;
        }

        public void AddRibbons(string bossId, int amount)
        {
            if (string.IsNullOrEmpty(bossId) || amount <= 0)
            {
                return;
            }

            _ribbons[bossId] = RibbonsOf(bossId) + amount;
            EventBus.Publish(new MountCollectionChangedEvent());
        }

        /// <summary>Trades the boss's Fitas do Laço for <paramref name="mountId"/> at the Haras (25 Rare / 80 Epic). Null when not allowed.</summary>
        public OwnedMount RedeemRibbons(string bossId, string mountId)
        {
            if (!MountDropRules.CanRedeem(bossId, mountId, RibbonsOf(bossId)))
            {
                return null;
            }

            _ribbons[bossId] = RibbonsOf(bossId) - MountDropRules.CostOf(bossId, mountId);
            return Grant(mountId, SourceRibbon);
        }

        /// <summary>
        /// A quest was completed / achievement unlocked / map secret claimed: grants the matching
        /// <see cref="MountSourceHooks"/> mount once. Returns the granted mounts.
        /// </summary>
        public List<OwnedMount> ReportHook(MountSourceKind kind, string hookId)
        {
            var granted = new List<OwnedMount>();
            foreach (MountSourceHook hook in MountSourceHooks.Matching(kind, hookId).ToList())
            {
                if (hook.Kind != MountSourceKind.Event && MountSourceHooks.ShouldGrant(hook, Owns(hook.MountId)))
                {
                    OwnedMount mount = Grant(hook.MountId, hook.SourceTag);
                    if (mount != null)
                    {
                        granted.Add(mount);
                    }
                }
            }

            return granted;
        }

        /// <summary>Festa Junina shop: pays <paramref name="prendas"/> for an event mount (Ema de Fogo 90, Boi-Bumbá 250). Null when not affordable / not an event mount.</summary>
        public OwnedMount BuyWithPrendas(string mountId, ref int prendas)
        {
            int before = prendas;
            if (!MountSourceHooks.TryPayPrendas(mountId, ref prendas))
            {
                return null;
            }

            OwnedMount mount = Grant(mountId, SourceEvent);
            if (mount == null)
            {
                prendas = before;
            }

            return mount;
        }

        private static bool DefaultIsNight()
        {
            return ServiceLocator.TryGet(out Rapadura.Gameplay.Environment.TimeOfDayManager time) && time != null
                && Rapadura.Gameplay.Night.NightDangerRules.IsNight(time.Phase);
        }

        private void OnEnemyDied(Rapadura.Gameplay.Enemies.EnemyDiedEvent evt)
        {
            Rapadura.Gameplay.Enemies.EnemyController enemy = evt.Enemy != null ? evt.Enemy.GetComponent<Rapadura.Gameplay.Enemies.EnemyController>() : null;
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            ResolveEnemyDeath(enemy.Definition.name, evt.WasBoss, IsNight != null && IsNight());
        }

        private void OnQuestCompleted(Rapadura.Gameplay.Quests.QuestCompletedEvent evt) => ReportHook(MountSourceKind.Quest, evt.QuestId);

        private void OnAchievementUnlocked(Rapadura.Gameplay.Achievements.AchievementUnlockedEvent evt)
        {
            if (evt.Achievement != null)
            {
                ReportHook(MountSourceKind.Achievement, evt.Achievement.AchievementId);
            }
        }

        private void OnSecretClaimed(Rapadura.Gameplay.Secrets.SecretClaimedEvent evt) => ReportHook(MountSourceKind.Secret, evt.SecretId);

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            // +5 XP to the active mount that waited at the edge of a won fight (§4.1).
            OwnedMount active = Active;
            if (evt.Outcome == EncounterOutcome.Victory && active != null && !active.IsTired)
            {
                AddXp(active, MountCareRules.VictoryXp);
            }

            ApplyStable(MountStable.Current());
            RestAfterEncounter();
        }

        // ------------------------------------------------------------------
        // ISaveable
        // ------------------------------------------------------------------

        public object CaptureState()
        {
            string[] ribbonIds = _ribbons.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            return new MountCollectionSaveState
            {
                version = MountCollectionSaveState.CurrentVersion,
                activeMountGuid = _activeGuid ?? string.Empty,
                harasExtraSlots = _harasExtraSlots,
                mounts = _mounts.Select(Clone).ToList(),
                bossRibbonIds = ribbonIds.ToList(),
                bossRibbonCounts = ribbonIds.Select(id => _ribbons[id]).ToList(),
                codexSeen = _codexSeen.OrderBy(k => k, StringComparer.Ordinal).ToList(),
                ownedCosmetics = _cosmetics.OrderBy(k => k, StringComparer.Ordinal).ToList(),
                lastWaterDay = _lastWaterDay,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not MountCollectionSaveState saved)
            {
                return;
            }

            ResetForNewSession();
            if (saved.mounts != null)
            {
                foreach (OwnedMount mount in saved.mounts)
                {
                    if (mount != null && !string.IsNullOrEmpty(mount.mountId) && !string.IsNullOrEmpty(mount.guid) && Find(mount.guid) == null)
                    {
                        OwnedMount copy = Clone(mount);
                        copy.level = Mathf.Clamp(copy.level, 1, MountRules.MaxLevel);
                        copy.tiredUntilEncounter = Mathf.Clamp(copy.tiredUntilEncounter, 0, MountRules.TiredEncounters);
                        copy.satiety = Mathf.Clamp(copy.satiety, 0f, MountCareRules.MaxSatiety);
                        copy.affection = Mathf.Clamp(copy.affection, 0, MountCareRules.MaxAffection);
                        _mounts.Add(copy);
                        _codexSeen.Add(copy.mountId);
                    }
                }
            }

            _activeGuid = Find(saved.activeMountGuid) != null ? saved.activeMountGuid : _mounts.Count > 0 ? _mounts[0].guid : string.Empty;
            _harasExtraSlots = Mathf.Max(0, saved.harasExtraSlots);

            if (saved.bossRibbonIds != null && saved.bossRibbonCounts != null)
            {
                for (int i = 0; i < Math.Min(saved.bossRibbonIds.Count, saved.bossRibbonCounts.Count); i++)
                {
                    if (!string.IsNullOrEmpty(saved.bossRibbonIds[i]))
                    {
                        _ribbons[saved.bossRibbonIds[i]] = Math.Max(0, saved.bossRibbonCounts[i]);
                    }
                }
            }

            if (saved.codexSeen != null)
            {
                foreach (string id in saved.codexSeen)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        _codexSeen.Add(id);
                    }
                }
            }

            if (saved.ownedCosmetics != null)
            {
                foreach (string id in saved.ownedCosmetics)
                {
                    if (MountCosmetics.Get(id) != null)
                    {
                        _cosmetics.Add(id);
                    }
                }
            }

            _lastWaterDay = saved.lastWaterDay;
            EventBus.Publish(new MountCollectionChangedEvent());
        }

        private static OwnedMount Clone(OwnedMount m)
        {
            return JsonUtility.FromJson<OwnedMount>(JsonUtility.ToJson(m));
        }
    }
}
