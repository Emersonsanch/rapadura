using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts
{
    /// <summary>
    /// Every rule of the mounts as plain C# (Docs/Mounts.md §2, §4.1, §9.2, §12.2), EditMode-tested and meant to be
    /// reused by the online server, same pattern as <c>StunRules</c>/<c>SafeZoneRules</c>:
    /// <list type="bullet">
    /// <item>Speed: the species multiplier (×1.30..×1.80 of the 5.5 m/s run) × gallop ×1.20, so the fastest ride is
    /// ×2.16 (11.9 m/s), inside the <c>BuffController</c> cap of 2.5. Mounted, MoveSpeed buffs above 1 are ignored
    /// (Passo do Vento and the dash don't stack with the mount) but slows still apply; rooted stays rooted.</item>
    /// <item>Safe zone: trot only (×1.00, no gallop) and no abilities.</item>
    /// <item>Summon: a 1.5 s whistle, only with 5 s without dealing or taking damage, 10 s after the last dismount,
    /// never in an arena fight (M1: fights start on foot) nor indoors; damage, attacking, a skill or a jump break
    /// the whistle.</item>
    /// <item>Gallop: the mount's own stamina, 12/s spent, 15/s back; once empty it trots until
    /// <see cref="GallopResumeFraction"/> is back.</item>
    /// <item>Damage: half of what the rider takes (after defense) goes to the mount's health; at 0 the mount is
    /// "tired": the cousin rolls off unhurt and the mount walks home to rest for <see cref="TiredEncounters"/> fights.
    /// A mount never dies.</item>
    /// <item>Ability cooldowns (level −0.5%/level, 2 hearts −5%, favorite −15%), boss rules (fear/knockback ignore
    /// bosses, crowd control halves like <c>StunRules</c>), the §2.4 budget and the server speed cap.</item>
    /// </list>
    /// </summary>
    public static class MountRules
    {
        public const float HeroRunSpeed = 5.5f;
        public const float MinSpeedMultiplier = 1.30f;
        public const float MaxSpeedMultiplier = 1.80f;
        public const float GallopMultiplier = 1.20f;

        /// <summary>Inside a city: the mount walks at the hero's run speed (×1.00), no gallop.</summary>
        public const float SafeZoneTrotMultiplier = 1.0f;

        /// <summary>Same limits as <c>BuffController.MinMoveSpeedMultiplier/MaxMoveSpeedMultiplier</c>.</summary>
        public const float MinMoveMultiplier = 0.1f;
        public const float MaxMoveMultiplier = 2.5f;

        public const float MaxDefensePercent = 0.15f;
        public const float DefaultDamageShare = 0.5f;

        public const float SummonSeconds = 1.5f;
        public const float OutOfCombatSeconds = 5f;
        public const float RecallCooldownSeconds = 10f;

        public const float GallopStaminaDrainPerSecond = 12f;
        public const float GallopStaminaRegenPerSecond = 15f;

        /// <summary>After running dry, the gallop comes back once this fraction of the stamina is refilled.</summary>
        public const float GallopResumeFraction = 0.25f;

        /// <summary>Offline: a tired mount rests for this many encounters (online: <see cref="TiredSeconds"/>).</summary>
        public const int TiredEncounters = 2;
        public const float TiredSeconds = 120f;

        /// <summary>10% slack of the server speed check for latency (§9.2).</summary>
        public const float ServerSlack = 1.10f;

        /// <summary>BuffController source of the mount's defense bonus.</summary>
        public const string BuffSource = "mount";

        public const float MountedAttackPenalty = 0.8f;
        public const int MaxLevel = 20;

        /// <summary>Fastest ride the game allows: ×1.80 × 1.20 = ×2.16 of the run (11.9 m/s).</summary>
        public static float MaxRideMultiplier => MaxSpeedMultiplier * GallopMultiplier;

        // ------------------------------------------------------------------
        // Speed
        // ------------------------------------------------------------------

        /// <summary>
        /// The motor multiplier while mounted, applied to the run speed. <paramref name="buffMultiplier"/> is the
        /// rider's <c>BuffController.GetMoveSpeedMultiplier()</c> (0 = rooted): anything above 1 is ignored, slows
        /// scale the ride. Result clamped to [<see cref="MinMoveMultiplier"/>, <see cref="MaxMoveMultiplier"/>], or 0
        /// while rooted.
        /// </summary>
        public static float MountedSpeedMultiplier(float buffMultiplier, float mountSpeed, bool galloping, bool inSafeZone, bool hungry = false)
        {
            if (buffMultiplier <= 0f)
            {
                return 0f;
            }

            float slow = Mathf.Min(buffMultiplier, 1f);
            float ride = inSafeZone
                ? SafeZoneTrotMultiplier
                : hungry
                    ? Mathf.Min(MountCareRules.HungrySpeedMultiplier, Mathf.Clamp(mountSpeed, MinSpeedMultiplier, MaxSpeedMultiplier))
                    : Mathf.Clamp(mountSpeed, MinSpeedMultiplier, MaxSpeedMultiplier) * (galloping ? GallopMultiplier : 1f);
            return Mathf.Clamp(ride * slow, MinMoveMultiplier, MaxMoveMultiplier);
        }

        /// <summary>Server-side speed cap (m/s) of §9.2 for a species: run × species × gallop × slow × 10% slack.</summary>
        public static float ServerSpeedCap(float mountSpeed, bool galloping, float slow, bool inSafeZone)
        {
            float ride = MountedSpeedMultiplier(Mathf.Clamp(slow, MinMoveMultiplier, 1f), mountSpeed, galloping, inSafeZone);
            return HeroRunSpeed * ride * ServerSlack;
        }

        // ------------------------------------------------------------------
        // Where and when
        // ------------------------------------------------------------------

        /// <summary>Where the hero is for riding purposes.</summary>
        public static MountPlace ResolvePlace(bool onWorldMap, bool inSafeZone, bool arenaFightActive, bool indoors)
        {
            if (indoors)
            {
                return MountPlace.Indoors;
            }

            if (inSafeZone)
            {
                return MountPlace.SafeZone;
            }

            if (onWorldMap)
            {
                return MountPlace.WorldMap;
            }

            return arenaFightActive ? MountPlace.ArenaFight : MountPlace.ArenaFree;
        }

        /// <summary>Offline M1: the map, the cities and an arena with no fight running. An arena fight starts on foot; never indoors.</summary>
        public static bool CanRideIn(MountPlace place)
        {
            return place == MountPlace.WorldMap || place == MountPlace.SafeZone || place == MountPlace.ArenaFree;
        }

        /// <summary>"Em combate" = dealt or took damage less than <see cref="OutOfCombatSeconds"/> ago.</summary>
        public static bool IsInCombat(float secondsSinceCombat) => secondsSinceCombat >= 0f && secondsSinceCombat < OutOfCombatSeconds;

        /// <summary>Whether the whistle may start, or the first reason why not.</summary>
        public static MountSummonBlock CanSummon(MountPlace place, bool hasActiveMount, bool mounted, bool tired, float secondsSinceCombat,
            float recallCooldownRemaining)
        {
            if (!hasActiveMount)
            {
                return MountSummonBlock.NoMount;
            }

            if (mounted)
            {
                return MountSummonBlock.AlreadyMounted;
            }

            if (!CanRideIn(place))
            {
                return MountSummonBlock.NotHere;
            }

            if (tired)
            {
                return MountSummonBlock.Tired;
            }

            if (IsInCombat(secondsSinceCombat))
            {
                return MountSummonBlock.InCombat;
            }

            return recallCooldownRemaining > 0f ? MountSummonBlock.RecallCooldown : MountSummonBlock.None;
        }

        /// <summary>The whistle breaks when the hero takes damage, attacks, uses a skill or jumps.</summary>
        public static bool SummonInterrupted(bool tookDamage, bool attacked, bool usedSkill, bool jumped)
        {
            return tookDamage || attacked || usedSkill || jumped;
        }

        /// <summary>Whistle progress 0..1 after holding for <paramref name="heldSeconds"/> (<paramref name="summonSeconds"/> ≤ 0 = instant).</summary>
        public static float SummonProgress(float heldSeconds, float summonSeconds = SummonSeconds)
        {
            return summonSeconds <= 0f ? 1f : Mathf.Clamp01(heldSeconds / summonSeconds);
        }

        /// <summary>Entering combat in a city drops the rider (§2.5 "Divisa da zona segura"); offline it also covers a fight starting.</summary>
        public static bool ShouldForceDismount(MountPlace place, bool inCombat)
        {
            return !CanRideIn(place) || (place == MountPlace.SafeZone && inCombat);
        }

        // ------------------------------------------------------------------
        // Gallop stamina
        // ------------------------------------------------------------------

        /// <summary>Gallop is never allowed in a city; otherwise only with stamina and not while recovering from running dry.</summary>
        public static bool CanGallop(MountPlace place, float stamina, bool exhausted, bool hungry = false)
        {
            return place != MountPlace.SafeZone && !hungry && !exhausted && stamina > 0f;
        }

        /// <summary>
        /// One step of the mount's gallop stamina. <paramref name="galloping"/> tells whether this step gallops
        /// (wanted, allowed, stamina left); running dry sets <paramref name="exhausted"/> until
        /// <see cref="GallopResumeFraction"/> of <paramref name="maxStamina"/> is back.
        /// </summary>
        public static float StepGallopStamina(float stamina, float maxStamina, bool wantsGallop, bool gallopAllowed, float deltaTime,
            ref bool exhausted, out bool galloping)
        {
            maxStamina = Mathf.Max(1f, maxStamina);
            stamina = Mathf.Clamp(stamina, 0f, maxStamina);
            galloping = wantsGallop && gallopAllowed && !exhausted && stamina > 0f;
            if (deltaTime <= 0f)
            {
                return stamina;
            }

            if (galloping)
            {
                stamina -= GallopStaminaDrainPerSecond * deltaTime;
                if (stamina <= 0f)
                {
                    stamina = 0f;
                    exhausted = true;
                }
            }
            else
            {
                stamina = Mathf.Min(maxStamina, stamina + GallopStaminaRegenPerSecond * deltaTime);
            }

            if (exhausted && stamina >= maxStamina * GallopResumeFraction)
            {
                exhausted = false;
            }

            return stamina;
        }

        /// <summary>Seconds of gallop a full bar gives (100 = 8.3 s).</summary>
        public static float GallopSeconds(float stamina) => Mathf.Max(0f, stamina) / GallopStaminaDrainPerSecond;

        // ------------------------------------------------------------------
        // Damage and tiredness
        // ------------------------------------------------------------------

        /// <summary>
        /// Splits damage the rider takes (already after defense): <paramref name="share"/> of it goes to the mount,
        /// never more than the mount's remaining health; the rest stays with the hero. Returns the mount's health after.
        /// </summary>
        public static float SplitDamage(float damage, float share, float mountHealth, out float toHero, out float toMount)
        {
            damage = Mathf.Max(0f, damage);
            mountHealth = Mathf.Max(0f, mountHealth);
            toMount = Mathf.Min(damage * Mathf.Clamp01(share), mountHealth);
            toHero = damage - toMount;
            return mountHealth - toMount;
        }

        /// <summary>A mount at 0 health becomes tired (it never dies).</summary>
        public static bool BecomesTired(float mountHealth) => mountHealth <= 0.0001f;

        public static bool IsTired(int tiredEncountersLeft) => tiredEncountersLeft > 0;

        /// <summary>One encounter ended: the rest counts down; returns the encounters still to rest.</summary>
        public static int RestAfterEncounter(int tiredEncountersLeft) => Mathf.Max(0, tiredEncountersLeft - 1);

        // ------------------------------------------------------------------
        // Level, cooldowns, abilities
        // ------------------------------------------------------------------

        public static float MaxHealthAt(float baseHealth, int level) => baseHealth * (1f + 0.05f * (Mathf.Clamp(level, 1, MaxLevel) - 1));

        public static float MaxStaminaAt(float baseStamina, int level) => baseStamina + (Mathf.Clamp(level, 1, MaxLevel) - 1);

        /// <summary>Ability cooldown for a mount: −0.5% per level above 1, −5% from 2 hearts, −15% for the rider's favorite.</summary>
        public static float AbilityCooldownSeconds(float baseCooldown, int level, int hearts, bool isFavorite)
        {
            float levelFactor = 1f - 0.005f * (Mathf.Clamp(level, 1, MaxLevel) - 1);
            float heartFactor = hearts >= 2 ? 0.95f : 1f;
            float favoriteFactor = isFavorite ? 0.85f : 1f;
            return Mathf.Max(0f, baseCooldown) * levelFactor * heartFactor * favoriteFactor;
        }

        /// <summary>Time (same clock as <paramref name="now"/>) at which an ability used at <paramref name="now"/> is ready again.</summary>
        public static float CooldownReadyAt(float now, float cooldownSeconds) => now + Mathf.Max(0f, cooldownSeconds);

        public static float CooldownRemaining(float readyAt, float now) => Mathf.Max(0f, readyAt - now);

        public static bool IsReady(float readyAt, float now) => now >= readyAt;

        /// <summary>Abilities need the rider mounted, the cooldown over and (always, §2.5) to be outside a city.</summary>
        public static bool CanUseAbility(MountPlace place, bool mounted, bool cooldownReady, bool allowedInSafeZone = false)
        {
            if (!mounted || !cooldownReady || !CanRideIn(place))
            {
                return false;
            }

            return place != MountPlace.SafeZone || allowedInSafeZone;
        }

        /// <summary>Duration an effect has on a target: bosses take <paramref name="bossMultiplier"/> (0 = immune: fear, knockback).</summary>
        public static float EffectDurationOn(float duration, bool isBoss, float bossMultiplier)
        {
            return Mathf.Max(0f, duration) * (isBoss ? Mathf.Clamp01(bossMultiplier) : 1f);
        }

        /// <summary>Mounted basic attack damage multiplier (M4): −20%, war mounts lose nothing.</summary>
        public static float MountedAttackMultiplier(bool isWarMount) => isWarMount ? 1f : MountedAttackPenalty;

        // ------------------------------------------------------------------
        // Budget (§2.4)
        // ------------------------------------------------------------------

        /// <summary>pontos = (velocidade − 1,30) / 0,05 + defesa% + (vida − 100) / 25 + (fôlego − 100) / 10.</summary>
        public static float BudgetPoints(float speed, float defenseFraction, float health, float stamina)
        {
            return (speed - MinSpeedMultiplier) / 0.05f + defenseFraction * 100f + (health - 100f) / 25f + (stamina - 100f) / 10f;
        }

        public static int BudgetFor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return 8;
                case ItemRarity.Uncommon: return 12;
                case ItemRarity.Rare: return 16;
                case ItemRarity.Epic: return 21;
                default: return 26;
            }
        }
    }
}
