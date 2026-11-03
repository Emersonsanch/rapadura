using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dungeon
{
    /// <summary>What opening a sarcophagus gives (<see cref="DungeonRules.RollSarcophagus"/>).</summary>
    public enum SarcophagusOutcome
    {
        Loot = 0,
        Ambush = 1,
    }

    /// <summary>The Colosso de Crânios's attack phases, by health (<see cref="DungeonRules.ColossusPhaseFor"/>).</summary>
    public enum ColossusPhase
    {
        /// <summary>100%–70%: club slam with a radial shockwave.</summary>
        Slam = 1,

        /// <summary>70%–30%: three bouncing skulls thrown down the hall.</summary>
        Skulls = 2,

        /// <summary>Below 30%: the rib cage around the hero while a charged blow builds up.</summary>
        Cage = 3,
    }

    /// <summary>Result of <see cref="DungeonRules.TryUnlockGate"/>.</summary>
    public enum GateUnlockResult
    {
        /// <summary>Opened now (the key was spent when the gate consumes it).</summary>
        Unlocked = 0,

        /// <summary>Already open in this save: nothing asked, nothing spent.</summary>
        AlreadyOpen = 1,

        /// <summary>No key in the bag: nothing changes.</summary>
        NoKey = 2,
    }

    /// <summary>
    /// Every number and rule of the Ossuário dos Artífices (Docs/Dungeons.md), pure and EditMode-tested: entry, the gates and
    /// their keys, the sarcophagi's 70/30 roll, the bone wall's blunt-impact rule, each enemy's ability timers and the
    /// boss's phases, the "Toque Espectral" set bonus and the anti-exploit rule of the reanimated guardians (0 XP, no drop).
    /// Randomness always comes in as a roll in [0, 1) or a <see cref="System.Random"/> the caller owns (online: the server),
    /// so the same rules run on a future server unchanged.
    /// </summary>
    public static class DungeonRules
    {
        // ------------------------------------------------------------------
        // Identity and entry
        // ------------------------------------------------------------------

        /// <summary>Id of the dungeon in <c>EncounterPayload.dungeonId</c> and in the save.</summary>
        public const string OssuaryId = "ossuario";

        /// <summary>The ossuary is reached from the Grutas do Limiar (the cave region of the world map).</summary>
        public const BiomeType Region = BiomeType.Caves;

        /// <summary>Hero level needed to go in (the enemies are a little above the caves' regulars).</summary>
        public const int MinLevel = 12;

        /// <summary>Resources folder of the dungeon's EnemyDefinition assets (outside Resources/Enemies: they are not in any region's random table).</summary>
        public const string EnemyResourcesFolder = "Dungeons/Ossuario/Enemies";

        /// <summary>The catacombs' loot table (a BiomeDefinition used only for its loot entries, rolled by EnemyLootRoller).</summary>
        public const string LootTableResourcePath = "Dungeons/Ossuario/LootTable_Catacombs";

        public const string FloatingSkullId = "enemy_floating_skull";
        public const string SkeletonWarriorId = "enemy_skeleton_warrior";
        public const string SkeletonArcherId = "enemy_skeleton_archer";
        public const string BoneSorcererId = "enemy_bone_sorcerer";
        public const string BoneColossusId = "boss_bone_colossus";

        /// <summary>The five enemies, regulars first.</summary>
        public static readonly IReadOnlyList<string> EnemyIds = new[] { FloatingSkullId, SkeletonWarriorId, SkeletonArcherId, BoneSorcererId, BoneColossusId };

        public const string BoneKeyItemId = "item_bone_key";
        public const string BoneChestItemId = "armor_bone_chest";
        public const string BoneHelmItemId = "armor_bone_helm";
        public const string BoneFragmentItemId = "item_bone_fragment";
        public const string HoledCoinItemId = "item_qocho_holed_coin";
        public const string RustySwordItemId = "item_rusty_watch_sword";

        public static string EnemyResourcePath(string enemyId) => EnemyResourcesFolder + "/" + enemyId;

        /// <summary>Whether a hero of <paramref name="heroLevel"/> standing in <paramref name="region"/> may go in.</summary>
        public static bool CanEnter(int heroLevel, BiomeType? region) => region == Region && heroLevel >= MinLevel;

        // ------------------------------------------------------------------
        // Gates, sarcophagi, wall, chest (ids are save keys: never rename)
        // ------------------------------------------------------------------

        public const string GateGalleryId = "ossuario.gate.galeria";
        public const string GateColossusId = "ossuario.gate.colosso";
        public static readonly IReadOnlyList<string> GateIds = new[] { GateGalleryId, GateColossusId };

        public static readonly IReadOnlyList<string> SarcophagusIds = new[]
        {
            "ossuario.sarc.vestibulo.0", "ossuario.sarc.vestibulo.1", "ossuario.sarc.galeria.0", "ossuario.sarc.galeria.1",
        };

        public const string BoneWallId = "ossuario.wall.nicho";
        public const string LegendaryChestId = "ossuario.chest.nicho";

        /// <summary>
        /// The gate's rule: an open gate never asks again; a closed one opens only with a key in the bag, spending it when
        /// <paramref name="consumeKey"/> (and failing — nothing changes — if the key cannot be removed). Pure: the bag is
        /// two callbacks, so tests and a server can drive it.
        /// </summary>
        public static GateUnlockResult TryUnlockGate(DungeonProgress progress, string gateId, string keyItemId, bool consumeKey,
            Func<string, int> countHeld, Func<string, int, bool> removeItem)
        {
            if (progress != null && progress.IsGateOpen(gateId))
            {
                return GateUnlockResult.AlreadyOpen;
            }

            int held = countHeld != null && !string.IsNullOrEmpty(keyItemId) ? countHeld(keyItemId) : 0;
            if (held <= 0)
            {
                return GateUnlockResult.NoKey;
            }

            if (consumeKey && (removeItem == null || !removeItem(keyItemId, 1)))
            {
                return GateUnlockResult.NoKey;
            }

            progress?.MarkGateOpen(gateId);
            return GateUnlockResult.Unlocked;
        }

        /// <summary>Chance a sarcophagus holds loot; the rest wake <see cref="SarcophagusAmbushSkulls"/> floating skulls.</summary>
        public const float SarcophagusLootChance = 0.7f;
        public const int SarcophagusAmbushSkulls = 2;

        /// <summary>Rolls of the catacombs' table a looted sarcophagus gives (each roll always lands).</summary>
        public const int SarcophagusLootRolls = 2;

        /// <summary>Rolls of the table the legendary chest behind the bone wall gives, on top of its fixed piece.</summary>
        public const int LegendaryChestRolls = 3;

        public static SarcophagusOutcome RollSarcophagus(double roll) => roll < SarcophagusLootChance ? SarcophagusOutcome.Loot : SarcophagusOutcome.Ambush;

        /// <summary>Distance (m, flat) at which the hero opens a sarcophagus / tries a gate by walking up to it.</summary>
        public const float InteractRadius = 2.1f;

        /// <summary>The hero must step this far away before the same prop reacts again (no spam while standing next to it).</summary>
        public const float InteractRearmRadius = 3.2f;

        /// <summary>
        /// Whether a skill cast is a blunt impact that brings the bone wall down at once: an Earth area skill (Joaquim's
        /// Maremoto and friends: the same rule as the ground strikes, <c>GroundImpactSystem.TryGetSkillStrike</c>) or one of
        /// the explosive skills (Ícaro's Granada de Faísca, the generic Explosão).
        /// </summary>
        public static bool IsBluntImpact(string skillId, ElementType element, float areaRadius)
        {
            if (element == ElementType.Earth && areaRadius > 0f)
            {
                return true;
            }

            return skillId == "skill_icaro_granada_eletrica" || skillId == "skill_explosion";
        }

        /// <summary>Extra reach (m) of a blunt impact on top of its area radius when checking the wall.</summary>
        public const float BluntReachMargin = 1.5f;

        /// <summary>
        /// Plain hits that crumble the wall without a blunt skill (kid-friendly: every cousin can open the niche, the
        /// blunt skills just do it in one go).
        /// </summary>
        public const int WallHitsToBreak = 8;

        // ------------------------------------------------------------------
        // Enemies
        // ------------------------------------------------------------------

        /// <summary>Caveira Flutuante: height it floats at, its slow sine bob and the quick back-off after a dive.</summary>
        public const float SkullHoverHeight = 1.2f;
        public const float SkullBobAmplitude = 0.18f;
        public const float SkullBobHz = 0.55f;
        public const float SkullRetreatDistance = 2f;
        public const float SkullRetreatSeconds = 0.3f;

        /// <summary>Its fairy light (point light on the skull).</summary>
        public const float SkullLightIntensity = 1.5f;
        public const float SkullLightRange = 3f;

        /// <summary>Vertical bob (m) of the skull at <paramref name="time"/> seconds (a sine around the hover height's rest).</summary>
        public static float SkullBob(float time) => Mathf.Sin(time * SkullBobHz * 2f * Mathf.PI) * SkullBobAmplitude;

        /// <summary>Guerreiro Esqueleto: permanent armour (percent Defense) and the bone-shield guard cycle.</summary>
        public const float WarriorArmor = 0.25f;
        public const float GuardIntervalSeconds = 6f;
        public const float GuardDurationSeconds = 2.5f;
        public const float GuardDamageReduction = 0.6f;
        public const float GuardProjectileReflect = 0.15f;

        /// <summary>A hit from farther than this (m, flat) counts as a projectile for the guard's reflection.</summary>
        public const float RangedHitDistance = 3.5f;

        /// <summary>Whether the warrior holds its guard <paramref name="secondsAlive"/> after spawning: the last 2.5 s of every 6 s.</summary>
        public static bool IsGuarding(float secondsAlive)
        {
            if (secondsAlive < 0f)
            {
                return false;
            }

            float intoCycle = secondsAlive % GuardIntervalSeconds;
            return intoCycle >= GuardIntervalSeconds - GuardDurationSeconds;
        }

        /// <summary>Damage the guard sends back to the shooter: 15% of a projectile's damage taken while guarding, else 0.</summary>
        public static float GuardReflectDamage(float damageTaken, bool ranged, bool guarding)
        {
            return guarding && ranged && damageTaken > 0f ? damageTaken * GuardProjectileReflect : 0f;
        }

        /// <summary>Arqueiro Esqueleto: range, the keep-away rule and its rib arrow.</summary>
        public const float ArcherRange = 14f;
        public const float ArcherRetreatTrigger = 4f;
        public const float ArcherRetreatDistance = 2.4f;
        public const float ArrowSpeed = 15f;
        public const float ArrowSlowFraction = 0.2f;
        public const float ArrowSlowSeconds = 3f;

        /// <summary>The archer backs off (three steps) while the hero is closer than <see cref="ArcherRetreatTrigger"/>.</summary>
        public static bool ArcherShouldRetreat(float flatDistanceToHero) => flatDistanceToHero < ArcherRetreatTrigger;

        /// <summary>Feiticeiro das Ossadas: spectral orbs and "Erguer as Ossadas".</summary>
        public const float OrbSpeed = 9f;
        public const float RaiseIntervalSeconds = 12f;
        public const float RaiseChannelSeconds = 2f;
        public const int RaiseMaxPerCast = 2;
        public const float RaiseHealthFraction = 0.4f;
        public const float RaiseRadius = 14f;

        /// <summary>
        /// Which fallen warriors one "Erguer as Ossadas" relights: the nearest ones within <see cref="RaiseRadius"/> of the
        /// sorcerer, up to <see cref="RaiseMaxPerCast"/>. Returns indices into <paramref name="corpses"/> (nearest first).
        /// </summary>
        public static List<int> PickRaiseTargets(Vector3 sorcerer, IReadOnlyList<Vector3> corpses)
        {
            var picked = new List<int>();
            if (corpses == null)
            {
                return picked;
            }

            var candidates = new List<(int index, float distance)>();
            for (int i = 0; i < corpses.Count; i++)
            {
                Vector3 offset = corpses[i] - sorcerer;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance <= RaiseRadius)
                {
                    candidates.Add((i, distance));
                }
            }

            candidates.Sort((a, b) => a.distance != b.distance ? a.distance.CompareTo(b.distance) : a.index.CompareTo(b.index));
            for (int i = 0; i < candidates.Count && picked.Count < RaiseMaxPerCast; i++)
            {
                picked.Add(candidates[i].index);
            }

            return picked;
        }

        /// <summary>
        /// Golden rule (anti-exploit): a reanimated guardian pays no kill XP, whatever the base; everything else pays its base.
        /// Applied through <c>IKillExperienceScaler</c> before every other XP multiplier, so 0 stays 0.
        /// </summary>
        public static int KillExperience(int baseExperience, bool isReanimated) => isReanimated ? 0 : Math.Max(0, baseExperience);

        /// <summary>The other half of the rule: a reanimated guardian drops nothing (no table roll, no gold, no key).</summary>
        public static bool DropsLoot(bool isReanimated) => !isReanimated;

        /// <summary>Colosso de Crânios.</summary>
        public const float ColossusSlamPhaseEnd = 0.7f;
        public const float ColossusCagePhaseStart = 0.3f;
        public const float ShockwaveRadius = 5f;
        public const float ShockwaveDamageFactor = 0.5f;
        public const float ShockwaveSeconds = 0.55f;
        public const int BouncingSkullsPerThrow = 3;
        public const float SkullThrowIntervalSeconds = 7f;
        public const float BouncingSkullSpeed = 7f;
        public const float BouncingSkullFuseSeconds = 3.2f;
        public const int BouncingSkullMaxBounces = 3;
        public const float BouncingSkullBlastRadius = 1.6f;
        public const float BouncingSkullDamage = 14f;
        public const int CageHitsToBreak = 3;
        public const float CageIntervalSeconds = 11f;
        public const float CageChargeSeconds = 4f;
        public const float CageChargedDamage = 44f;

        public static ColossusPhase ColossusPhaseFor(float healthFraction)
        {
            if (healthFraction > ColossusSlamPhaseEnd)
            {
                return ColossusPhase.Slam;
            }

            return healthFraction >= ColossusCagePhaseStart ? ColossusPhase.Skulls : ColossusPhase.Cage;
        }

        /// <summary>Spread (deg) of the three skulls around the throw direction.</summary>
        public static float SkullThrowAngle(int index, int count)
        {
            if (count <= 1)
            {
                return 0f;
            }

            const float spread = 50f;
            return -spread * 0.5f + spread * index / (count - 1);
        }

        /// <summary>
        /// One step of a bouncing skull down a hall of half width <paramref name="halfWidth"/> (lateral axis = local x):
        /// moves <paramref name="lateral"/> by <paramref name="lateralVelocity"/>·dt and reflects it off a side wall.
        /// Returns true when it bounced this step.
        /// </summary>
        public static bool StepBounce(ref float lateral, ref float lateralVelocity, float deltaTime, float halfWidth)
        {
            lateral += lateralVelocity * deltaTime;
            if (lateral > halfWidth)
            {
                lateral = 2f * halfWidth - lateral;
                lateralVelocity = -Mathf.Abs(lateralVelocity);
                return true;
            }

            if (lateral < -halfWidth)
            {
                lateral = -2f * halfWidth - lateral;
                lateralVelocity = Mathf.Abs(lateralVelocity);
                return true;
            }

            return false;
        }

        /// <summary>The charged blow of the cage phase lands only if the cage still holds the hero when it completes.</summary>
        public static float CageBlowDamage(bool cageStillHolding) => cageStillHolding ? CageChargedDamage : 0f;

        // ------------------------------------------------------------------
        // Armadura de Marfim dos Artífices
        // ------------------------------------------------------------------

        public static readonly IReadOnlyList<string> BoneSetItemIds = new[] { BoneChestItemId, BoneHelmItemId };
        public const int SpectralTouchPieces = 2;
        public const float SpectralTouchChance = 0.15f;
        public const float SpectralShardDamageFraction = 0.35f;
        public const float SpectralShardRadius = 4f;
        public const int SpectralShardMaxTargets = 3;

        /// <summary>Distinct pieces of the bone set among <paramref name="equippedItemIds"/>.</summary>
        public static int CountBoneSetPieces(IEnumerable<string> equippedItemIds)
        {
            if (equippedItemIds == null)
            {
                return 0;
            }

            var seen = new HashSet<string>();
            foreach (string id in equippedItemIds)
            {
                if (id != null && (id == BoneChestItemId || id == BoneHelmItemId))
                {
                    seen.Add(id);
                }
            }

            return seen.Count;
        }

        /// <summary>"Toque Espectral": with both pieces on, a critical hit has 15% (<paramref name="roll"/> in [0,1)) to throw a bone shard.</summary>
        public static bool SpectralTouchProcs(int setPieces, bool critical, double roll)
        {
            return critical && setPieces >= SpectralTouchPieces && roll < SpectralTouchChance;
        }

        /// <summary>Damage of each shard: 35% of the critical hit's damage (never below 1 for a positive hit).</summary>
        public static float SpectralShardDamage(float criticalDamage)
        {
            return criticalDamage > 0f ? Mathf.Max(1f, criticalDamage * SpectralShardDamageFraction) : 0f;
        }
    }
}
