using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.EventRooms
{
    /// <summary>
    /// The timed event rooms (Docs/Referencias_MMO.md §2, FASE 16). Values are serialized in
    /// <c>EncounterPayload.eventRoom</c> and the save, so never renumber.
    /// </summary>
    public enum EventRoomKind
    {
        None = 0,

        /// <summary>"Pátio dos Mazans" (Devil Square-like): timed waves, score by kills and combos.</summary>
        Terreiro = 1,

        /// <summary>"Torre da Roda das Doze Jarras" (Blood Castle-like): bridge → gate → Ullikummi → deliver the Jarra de Ouro.</summary>
        Torre = 2,
    }

    /// <summary>Result tier of a run, by score. Bronze..Mestre pay rewards; None pays nothing.</summary>
    public enum EventRoomTier
    {
        None = 0,
        Bronze = 1,
        Prata = 2,
        Ouro = 3,
        Mestre = 4,
    }

    /// <summary>Phases of the Torre da Roda das Doze Jarras, in order.</summary>
    public enum TorrePhase
    {
        /// <summary>Defeat <see cref="EventRoomRules.TorreKillsToOpenBridge"/> enemies to lower the bridge.</summary>
        OpenBridge = 0,

        /// <summary>Break the tower gate (a destructible object with HP).</summary>
        BreakGate = 1,

        /// <summary>Beat the tower's guardian (mini-boss).</summary>
        Guardian = 2,

        /// <summary>Pick up the Jarra de Ouro the guardian left behind.</summary>
        PickUpMoenda = 3,

        /// <summary>Carry it back to Adda at the entrance.</summary>
        Deliver = 4,

        /// <summary>Delivered: the room is cleared.</summary>
        Done = 5,
    }

    /// <summary>
    /// When one room opens: every <see cref="PeriodHours"/> in-game hours starting at <see cref="OffsetHours"/>, for
    /// <see cref="WindowHours"/> in-game hours. Serializable so the save can carry "the current schedule" (and, online,
    /// the server can push its own: it owns the schedule there).
    /// </summary>
    [Serializable]
    public struct EventRoomSchedule
    {
        public float periodHours;
        public float offsetHours;
        public float windowHours;

        public EventRoomSchedule(float periodHours, float offsetHours, float windowHours)
        {
            this.periodHours = periodHours;
            this.offsetHours = offsetHours;
            this.windowHours = windowHours;
        }

        public float PeriodHours => periodHours;
        public float OffsetHours => offsetHours;
        public float WindowHours => windowHours;

        /// <summary>Usable: positive period, a window shorter than the period.</summary>
        public bool IsValid => periodHours > 0f && windowHours > 0f && windowHours < periodHours && offsetHours >= 0f;
    }

    /// <summary>Where one room's schedule stands at a moment of in-game time.</summary>
    public readonly struct EventRoomWindow
    {
        /// <summary>Inside the entry window now.</summary>
        public readonly bool IsOpen;

        /// <summary>Index of the window that is open now, or of the last one that opened (-1 before the first).</summary>
        public readonly long WindowIndex;

        /// <summary>In-game hours until the window closes (open) or until the next one opens (closed).</summary>
        public readonly float HoursRemaining;

        /// <summary>In-game hour of day (0–24) when the window closes (open) or the next one opens (closed).</summary>
        public readonly float NextChangeHourOfDay;

        public EventRoomWindow(bool isOpen, long windowIndex, float hoursRemaining, float nextChangeHourOfDay)
        {
            IsOpen = isOpen;
            WindowIndex = windowIndex;
            HoursRemaining = hoursRemaining;
            NextChangeHourOfDay = nextChangeHourOfDay;
        }
    }

    /// <summary>One level bracket: which campaign tier the room's enemies are scaled to.</summary>
    public readonly struct EventRoomBracket
    {
        public readonly int Index;
        public readonly int MinLevel;

        /// <summary>Inclusive; int.MaxValue for the last bracket.</summary>
        public readonly int MaxLevel;

        /// <summary>Region tier the enemies are scaled to (0 Floresta … 3 Cavernas).</summary>
        public readonly int ReferenceTier;

        /// <summary>Extra health/XP on top of the reference tier (the last bracket, NG+ heroes).</summary>
        public readonly float ExtraMultiplier;

        public EventRoomBracket(int index, int minLevel, int maxLevel, int referenceTier, float extraMultiplier)
        {
            Index = index;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            ReferenceTier = referenceTier;
            ExtraMultiplier = extraMultiplier;
        }
    }

    /// <summary>How the enemies of a room are adjusted to the hero's bracket (applied by <c>EventRoomEnemyScaler</c>).</summary>
    public readonly struct EventRoomEnemyScale
    {
        public readonly float HealthMultiplier;

        /// <summary>PercentAdditive AttackDamage (0.2 = +20%).</summary>
        public readonly float DamageBonus;

        public readonly float ExperienceMultiplier;

        /// <summary>Visual size (the guardian is bigger).</summary>
        public readonly float VisualScale;

        public EventRoomEnemyScale(float healthMultiplier, float damageBonus, float experienceMultiplier, float visualScale)
        {
            HealthMultiplier = healthMultiplier;
            DamageBonus = damageBonus;
            ExperienceMultiplier = experienceMultiplier;
            VisualScale = visualScale;
        }

        public static EventRoomEnemyScale Identity => new EventRoomEnemyScale(1f, 0f, 1f, 1f);
    }

    /// <summary>What a finished run pays (before the bag decides what fits).</summary>
    public readonly struct EventRoomReward
    {
        public readonly EventRoomTier Tier;
        public readonly int Experience;
        public readonly int Gold;

        /// <summary>"Pedra-Faísca" (refino), granted only if the refining item exists.</summary>
        public readonly int BlessingStones;

        /// <summary>"Pedra da Alma Viva" (refino), granted only if the refining item exists.</summary>
        public readonly int SoulStones;

        public EventRoomReward(EventRoomTier tier, int experience, int gold, int blessingStones, int soulStones)
        {
            Tier = tier;
            Experience = experience;
            Gold = gold;
            BlessingStones = blessingStones;
            SoulStones = soulStones;
        }

        public bool IsEmpty => Experience <= 0 && Gold <= 0 && BlessingStones <= 0 && SoulStones <= 0;
    }

    /// <summary>What the Torre has achieved so far, for <see cref="EventRoomRules.TorreNextPhase"/>.</summary>
    public readonly struct TorreProgress
    {
        public readonly int Kills;
        public readonly bool GateBroken;
        public readonly bool GuardianDefeated;
        public readonly bool MoendaPickedUp;
        public readonly bool MoendaDelivered;

        public TorreProgress(int kills, bool gateBroken, bool guardianDefeated, bool moendaPickedUp, bool moendaDelivered)
        {
            Kills = kills;
            GateBroken = gateBroken;
            GuardianDefeated = guardianDefeated;
            MoendaPickedUp = moendaPickedUp;
            MoendaDelivered = moendaDelivered;
        }
    }

    /// <summary>
    /// Score of one run: points per kill times a combo multiplier (kills chained within
    /// <see cref="EventRoomRules.ComboWindowSeconds"/> of each other), plus phase/survival bonuses. Time is passed in
    /// (scaled game seconds since the run started), so it is EditMode-testable and pauses with the game.
    /// </summary>
    public sealed class EventRoomScore
    {
        private float _lastKillTime = float.NegativeInfinity;

        public int Total { get; private set; }
        public int Kills { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }

        /// <summary>Multiplier the next kill gets if it lands now (combo still alive) — for the HUD.</summary>
        public float CurrentMultiplier(float now) => EventRoomRules.ComboMultiplier(IsComboAlive(now) ? Combo + 1 : 1);

        public bool IsComboAlive(float now) => Combo > 0 && now - _lastKillTime <= EventRoomRules.ComboWindowSeconds;

        /// <summary>Counts a kill worth <paramref name="basePoints"/> at <paramref name="now"/>. Returns the points awarded.</summary>
        public int RegisterKill(float now, int basePoints)
        {
            Combo = IsComboAlive(now) ? Combo + 1 : 1;
            _lastKillTime = now;
            BestCombo = Math.Max(BestCombo, Combo);
            Kills++;
            int points = Mathf.RoundToInt(Math.Max(0, basePoints) * EventRoomRules.ComboMultiplier(Combo));
            Total += points;
            return points;
        }

        public void AddBonus(int points)
        {
            if (points > 0)
            {
                Total += points;
            }
        }
    }

    /// <summary>
    /// Every number and rule of the two event rooms, pure and EditMode-tested (EventRoomTests): schedule and entry
    /// windows, level brackets and enemy scaling, waves, scoring/combo, reward tiers and the Torre's phase machine.
    /// Numbers and reasoning: Docs/Balance/README.md, last section ("Salas de evento").
    ///
    /// Online (FASE 12): a room becomes a shared instance for up to <see cref="MaxPartySize"/> players (a party);
    /// every player keeps an own <see cref="EventRoomScore"/> (kills credited to the killer, phase bonuses to all
    /// present), tiers and rewards are per player, and the server owns the schedule (it pushes an
    /// <see cref="EventRoomSchedule"/> per room; clients only display it).
    /// </summary>
    public static class EventRoomRules
    {
        // ------------------------------------------------------------------
        // Schedule
        // ------------------------------------------------------------------

        public const float HoursPerDay = 24f;

        /// <summary>Terreiro opens at 0h, 4h, 8h, 12h, 16h and 20h for one in-game hour (~50 s real at the default 20-min day).</summary>
        public static readonly EventRoomSchedule TerreiroSchedule = new EventRoomSchedule(4f, 0f, 1f);

        /// <summary>Torre opens at 2h, 6h, 10h, 14h, 18h and 22h — staggered from the Terreiro so they never overlap.</summary>
        public static readonly EventRoomSchedule TorreSchedule = new EventRoomSchedule(4f, 2f, 1f);

        public const int MaxPartySize = 5;

        public static EventRoomSchedule DefaultSchedule(EventRoomKind kind) => kind == EventRoomKind.Torre ? TorreSchedule : TerreiroSchedule;

        /// <summary>In-game hours since day 1, 0h.</summary>
        public static double AbsoluteHours(int day, float hourOfDay)
        {
            return (Math.Max(1, day) - 1) * (double)HoursPerDay + Mathf.Clamp(hourOfDay, 0f, HoursPerDay);
        }

        public static EventRoomWindow WindowAt(EventRoomSchedule schedule, int day, float hourOfDay)
        {
            return WindowAt(schedule, AbsoluteHours(day, hourOfDay));
        }

        public static EventRoomWindow WindowAt(EventRoomSchedule schedule, double absoluteHours)
        {
            if (!schedule.IsValid)
            {
                return new EventRoomWindow(false, -1, float.PositiveInfinity, 0f);
            }

            double sinceOffset = absoluteHours - schedule.offsetHours;
            if (sinceOffset < 0d)
            {
                float untilFirst = (float)(-sinceOffset);
                return new EventRoomWindow(false, -1, untilFirst, HourOfDay(absoluteHours + untilFirst));
            }

            long index = (long)Math.Floor(sinceOffset / schedule.periodHours);
            float intoPeriod = (float)(sinceOffset - index * (double)schedule.periodHours);
            bool open = intoPeriod < schedule.windowHours;
            float remaining = open ? schedule.windowHours - intoPeriod : schedule.periodHours - intoPeriod;
            return new EventRoomWindow(open, index, remaining, HourOfDay(absoluteHours + remaining));
        }

        /// <summary>Whether a hero who entered window <paramref name="lastEnteredWindow"/> may enter now: open and not this same window.</summary>
        public static bool CanEnterWindow(EventRoomWindow window, long lastEnteredWindow)
        {
            return window.IsOpen && window.WindowIndex != lastEnteredWindow;
        }

        private static float HourOfDay(double absoluteHours)
        {
            double hour = absoluteHours % HoursPerDay;
            return (float)(hour < 0d ? hour + HoursPerDay : hour);
        }

        // ------------------------------------------------------------------
        // Entry
        // ------------------------------------------------------------------

        public const string TerreiroTicketItemId = "item_convite_terreiro";
        public const string TorreKeyItemId = "item_chave_engenho";

        public const int TerreiroMinLevel = 3;
        public const int TorreMinLevel = 6;

        public static string EntryItemId(EventRoomKind kind) => kind == EventRoomKind.Torre ? TorreKeyItemId : TerreiroTicketItemId;

        public static int MinLevel(EventRoomKind kind) => kind == EventRoomKind.Torre ? TorreMinLevel : TerreiroMinLevel;

        public static string RoomId(EventRoomKind kind) => kind == EventRoomKind.Torre ? "torre" : kind == EventRoomKind.Terreiro ? "terreiro" : string.Empty;

        public static EventRoomKind ParseRoomId(string id)
        {
            return id == "torre" ? EventRoomKind.Torre : id == "terreiro" ? EventRoomKind.Terreiro : EventRoomKind.None;
        }

        /// <summary>Chance a regular kill outside the rooms drops a Convite / a Chave (one roll each, Convite first).</summary>
        public const float TicketDropChanceCommon = 0.015f;
        public const float KeyDropChanceCommon = 0.0075f;

        /// <summary>Same for a boss (region boss challenge or a boss in a group).</summary>
        public const float TicketDropChanceBoss = 0.25f;
        public const float KeyDropChanceBoss = 0.15f;

        /// <summary>Which entry item one kill drops (rolls in [0,1)), or None. Never inside a room, never before the minimum level.</summary>
        public static EventRoomKind RollEntryDrop(bool wasBoss, bool insideEventRoom, int heroLevel, double ticketRoll, double keyRoll)
        {
            if (insideEventRoom || heroLevel < TerreiroMinLevel)
            {
                return EventRoomKind.None;
            }

            if (ticketRoll < (wasBoss ? TicketDropChanceBoss : TicketDropChanceCommon))
            {
                return EventRoomKind.Terreiro;
            }

            if (heroLevel >= TorreMinLevel && keyRoll < (wasBoss ? KeyDropChanceBoss : KeyDropChanceCommon))
            {
                return EventRoomKind.Torre;
            }

            return EventRoomKind.None;
        }

        // ------------------------------------------------------------------
        // Level brackets and enemy scaling
        // ------------------------------------------------------------------

        /// <summary>Health of the regular enemy of each region tier (Floresta 70, Deserto 110, Montanha 140, Cavernas 170: the assets).</summary>
        public static readonly IReadOnlyList<float> TierHealth = new[] { 70f, 110f, 140f, 170f };

        /// <summary>Kill XP of the regular enemy of each region tier (10 / 18 / 28 / 40: the assets).</summary>
        public static readonly IReadOnlyList<int> TierExperience = new[] { 10, 18, 28, 40 };

        public static readonly IReadOnlyList<EventRoomBracket> Brackets = new[]
        {
            new EventRoomBracket(0, 1, 4, 0, 1f),
            new EventRoomBracket(1, 5, 8, 1, 1f),
            new EventRoomBracket(2, 9, 12, 2, 1f),
            new EventRoomBracket(3, 13, 15, 3, 1f),
            new EventRoomBracket(4, 16, int.MaxValue, 3, 1.3f),
        };

        public static EventRoomBracket BracketFor(int level)
        {
            int clamped = Math.Max(1, level);
            foreach (EventRoomBracket bracket in Brackets)
            {
                if (clamped >= bracket.MinLevel && clamped <= bracket.MaxLevel)
                {
                    return bracket;
                }
            }

            return Brackets[Brackets.Count - 1];
        }

        public const float MinHealthMultiplier = 0.35f;
        public const float MaxHealthMultiplier = 3.5f;
        public const float MinDamageBonus = -0.4f;
        public const float MaxDamageBonus = 0.8f;

        /// <summary>
        /// Scales an enemy native to region tier <paramref name="nativeTier"/> to <paramref name="bracket"/>: health and
        /// XP by the ratio of the tiers' regular enemy (so a forest wolf in the Cavernas bracket has a bat swarm's health
        /// and pays a bat swarm's XP), damage by the square root (softer: many enemies hit at once in a room).
        /// </summary>
        public static EventRoomEnemyScale EnemyScale(int nativeTier, EventRoomBracket bracket)
        {
            int native = Mathf.Clamp(nativeTier, 0, TierHealth.Count - 1);
            int reference = Mathf.Clamp(bracket.ReferenceTier, 0, TierHealth.Count - 1);
            float extra = Mathf.Max(0.1f, bracket.ExtraMultiplier);

            float health = Mathf.Clamp(TierHealth[reference] * extra / TierHealth[native], MinHealthMultiplier, MaxHealthMultiplier);
            float damage = Mathf.Clamp(Mathf.Sqrt(health) - 1f, MinDamageBonus, MaxDamageBonus);
            float experience = TierExperience[reference] * extra / TierExperience[native];
            return new EventRoomEnemyScale(health, damage, experience, 1f);
        }

        /// <summary>The Torre's guardian: a regular enemy scaled like the others, then ×<see cref="GuardianHealthMultiplier"/> health/XP.</summary>
        public const float GuardianHealthMultiplier = 4f;
        public const float GuardianExtraDamageBonus = 0.25f;
        public const float GuardianVisualScale = 1.5f;

        /// <summary>Asset name of the enemy the guardian is built from ("Ushabti de Pedra", shown as Ullikummi; a regular enemy: no boss bookkeeping).</summary>
        public const string GuardianEnemyId = "enemy_mountain_stone_golem";

        public static EventRoomEnemyScale GuardianScale(int nativeTier, EventRoomBracket bracket)
        {
            EventRoomEnemyScale baseScale = EnemyScale(nativeTier, bracket);
            return new EventRoomEnemyScale(
                baseScale.HealthMultiplier * GuardianHealthMultiplier,
                Mathf.Clamp(baseScale.DamageBonus + GuardianExtraDamageBonus, MinDamageBonus, MaxDamageBonus + GuardianExtraDamageBonus),
                baseScale.ExperienceMultiplier * GuardianHealthMultiplier,
                GuardianVisualScale);
        }

        /// <summary>Kill XP after scaling (never below 1 for a positive base).</summary>
        public static int ScaleExperience(int baseExperience, float multiplier)
        {
            if (baseExperience <= 0)
            {
                return 0;
            }

            return Math.Max(1, Mathf.RoundToInt(baseExperience * Mathf.Max(0f, multiplier)));
        }

        // ------------------------------------------------------------------
        // Duration and waves
        // ------------------------------------------------------------------

        public const float TerreiroDurationSeconds = 180f;
        public const float TorreDurationSeconds = 240f;

        public static float DurationSeconds(EventRoomKind kind) => kind == EventRoomKind.Torre ? TorreDurationSeconds : TerreiroDurationSeconds;

        /// <summary>Terreiro: a wave every 20 s, or as soon as the field is down to one enemy.</summary>
        public const float TerreiroWaveIntervalSeconds = 20f;
        public const int TerreiroMaxAlive = 8;
        public const int TerreiroFirstWaveSize = 3;
        public const int TerreiroMaxWaveSize = 6;

        /// <summary>Torre, phase "Abrir a ponte": waves of 2, 3, 4… every 15 s.</summary>
        public const float TorreWaveIntervalSeconds = 15f;
        public const int TorreMaxAlive = 6;
        public const int TorreKillsToOpenBridge = 10;

        /// <summary>Torre after the bridge: a trickle of 2 every 18 s while breaking the gate / fighting the guardian, at most 3 alive.</summary>
        public const float TorreTrickleIntervalSeconds = 18f;
        public const int TorreTrickleSize = 2;
        public const int TorreTrickleMaxAlive = 3;

        /// <summary>Picking up the Moenda wakes 2 more (the race back to the entrance).</summary>
        public const int TorreAmbushSize = 2;

        /// <summary>Size of Terreiro wave <paramref name="waveIndex"/> (0-based): 3, 4, 5, 6, 6…</summary>
        public static int TerreiroWaveSize(int waveIndex) => Math.Min(TerreiroFirstWaveSize + Math.Max(0, waveIndex), TerreiroMaxWaveSize);

        /// <summary>Size of Torre bridge wave <paramref name="waveIndex"/> (0-based): 2, 3, 4, 4…</summary>
        public static int TorreWaveSize(int waveIndex) => Math.Min(2 + Math.Max(0, waveIndex), 4);

        /// <summary>
        /// Whether the next wave comes now: the field is (almost) clear (≤ 1 alive) or the interval has passed — and
        /// there is room under the alive cap.
        /// </summary>
        public static bool ShouldSpawnWave(int alive, float secondsSinceLastWave, float interval, int maxAlive)
        {
            if (alive >= maxAlive)
            {
                return false;
            }

            return alive <= 1 || secondsSinceLastWave >= interval;
        }

        /// <summary>How many of a wave of <paramref name="size"/> fit under the cap with <paramref name="alive"/> on the field.</summary>
        public static int FitWave(int size, int alive, int maxAlive) => Math.Max(0, Math.Min(size, maxAlive - Math.Max(0, alive)));

        // ------------------------------------------------------------------
        // Scoring
        // ------------------------------------------------------------------

        public const float ComboWindowSeconds = 4f;
        public const float ComboStep = 0.1f;
        public const float MaxComboMultiplier = 2f;

        public const int KillPoints = 100;
        public const int GuardianKillPoints = 1000;

        /// <summary>Terreiro: still standing when the timer ends.</summary>
        public const int TerreiroSurvivalBonus = 1000;

        public const int TorreBridgeBonus = 500;
        public const int TorreGateBonus = 1000;
        public const int TorreDeliveryBonus = 2000;

        /// <summary>Torre: points per whole second left on the timer at delivery.</summary>
        public const int TorreTimeBonusPerSecond = 10;

        /// <summary>×1.0 for the first kill of a chain, +0.1 per chained kill, up to ×2.0 (11 in a row).</summary>
        public static float ComboMultiplier(int combo)
        {
            return Mathf.Min(MaxComboMultiplier, 1f + ComboStep * Math.Max(0, combo - 1));
        }

        public static int TorreTimeBonus(float secondsLeft) => Math.Max(0, Mathf.FloorToInt(secondsLeft)) * TorreTimeBonusPerSecond;

        // ------------------------------------------------------------------
        // Tiers and rewards
        // ------------------------------------------------------------------

        /// <summary>Minimum score of Bronze, Prata, Ouro, Mestre (the same for both rooms).</summary>
        public static readonly IReadOnlyList<int> TierThresholds = new[] { 1000, 2500, 4500, 7000 };

        /// <summary>The high-score achievement asks for the Mestre threshold in any room.</summary>
        public static int HighScoreAchievementTarget => TierThresholds[TierThresholds.Count - 1];

        /// <summary>
        /// Tier of a finished run. The Torre without the Jarra delivered caps at Bronze (Blood Castle: no weapon, no
        /// prize); the Terreiro has no cap (dying just ends it earlier).
        /// </summary>
        public static EventRoomTier TierFor(int score, EventRoomKind kind, bool cleared)
        {
            var tier = EventRoomTier.None;
            for (int i = 0; i < TierThresholds.Count; i++)
            {
                if (score >= TierThresholds[i])
                {
                    tier = (EventRoomTier)(i + 1);
                }
            }

            if (kind == EventRoomKind.Torre && !cleared && tier > EventRoomTier.Bronze)
            {
                tier = EventRoomTier.Bronze;
            }

            return tier;
        }

        /// <summary>Tier reward XP, in kills of the bracket's regular enemy (None, Bronze, Prata, Ouro, Mestre).</summary>
        public static readonly IReadOnlyList<int> TierExperienceKills = new[] { 0, 5, 10, 16, 25 };

        /// <summary>Tier reward gold at bracket 0; +50% per bracket above.</summary>
        public static readonly IReadOnlyList<int> TierGold = new[] { 0, 8, 15, 25, 40 };

        public static readonly IReadOnlyList<int> TierBlessingStones = new[] { 0, 0, 1, 2, 2 };
        public static readonly IReadOnlyList<int> TierSoulStones = new[] { 0, 0, 0, 0, 1 };

        public static EventRoomReward RewardFor(EventRoomTier tier, EventRoomBracket bracket)
        {
            int t = Mathf.Clamp((int)tier, 0, TierExperienceKills.Count - 1);
            if (t == 0)
            {
                return new EventRoomReward(EventRoomTier.None, 0, 0, 0, 0);
            }

            int reference = Mathf.Clamp(bracket.ReferenceTier, 0, TierExperience.Count - 1);
            int experience = Mathf.RoundToInt(TierExperienceKills[t] * TierExperience[reference] * Mathf.Max(1f, bracket.ExtraMultiplier));
            int gold = Mathf.RoundToInt(TierGold[t] * (1f + 0.5f * Math.Max(0, bracket.Index)));
            return new EventRoomReward(tier, experience, gold, TierBlessingStones[t], TierSoulStones[t]);
        }

        /// <summary>Item ids of the refining stones (FASE 16 "Pedras da Luz Presa"); skipped while the items do not exist.</summary>
        public const string BlessingStoneItemId = "item_pedra_bencao";
        public const string SoulStoneItemId = "item_pedra_alma";

        /// <summary>Whether a run cleared the room: Terreiro = alive at the end of the timer; Torre = Jarra delivered.</summary>
        public static bool IsCleared(EventRoomKind kind, bool survivedToTimer, bool moendaDelivered)
        {
            return kind == EventRoomKind.Torre ? moendaDelivered : kind == EventRoomKind.Terreiro && survivedToTimer;
        }

        // ------------------------------------------------------------------
        // Torre phases
        // ------------------------------------------------------------------

        /// <summary>The phase the Torre is in for <paramref name="progress"/> (each step needs the previous one).</summary>
        public static TorrePhase TorrePhaseFor(TorreProgress progress)
        {
            if (progress.Kills < TorreKillsToOpenBridge)
            {
                return TorrePhase.OpenBridge;
            }

            if (!progress.GateBroken)
            {
                return TorrePhase.BreakGate;
            }

            if (!progress.GuardianDefeated)
            {
                return TorrePhase.Guardian;
            }

            if (!progress.MoendaPickedUp)
            {
                return TorrePhase.PickUpMoenda;
            }

            return progress.MoendaDelivered ? TorrePhase.Done : TorrePhase.Deliver;
        }

        /// <summary>The gate only takes damage once the bridge is down and until it breaks.</summary>
        public static bool GateAcceptsDamage(TorrePhase phase) => phase == TorrePhase.BreakGate;

        /// <summary>Gate health: six regular enemies of the bracket's tier (420 at bracket 0, 1326 at the last).</summary>
        public static float GateHealth(EventRoomBracket bracket)
        {
            int reference = Mathf.Clamp(bracket.ReferenceTier, 0, TierHealth.Count - 1);
            return 6f * TierHealth[reference] * Mathf.Max(1f, bracket.ExtraMultiplier);
        }

        /// <summary>Distance (m) at which the hero picks up the Jarra / hands it to Adda.</summary>
        public const float PickUpRadius = 1.8f;
        public const float DeliverRadius = 2.6f;
    }
}
